using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using TreeCmpWebAPI.Models.DTO;
using TreeCmpWebAPI.Repositories;

namespace TreeCmpWebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<IdentityUser> userManager;
        private readonly ITokenRepository tokenRepository;
        private readonly IEmailSender emailSender;

        public AuthController(UserManager<IdentityUser> userManager, ITokenRepository tokenRepository, IEmailSender emailSender)
        {
            this.userManager = userManager;
            this.tokenRepository = tokenRepository;
            this.emailSender = emailSender;
        }
        [HttpPost]
        [Route("Register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequestDto registerRequestDto)
        {
            var identityUser = new IdentityUser
            {
                UserName = registerRequestDto.Username,
                Email = registerRequestDto.Username,
                EmailConfirmed = false
            };

            var identityResult = await userManager.CreateAsync(identityUser, registerRequestDto.Password);

            if (!identityResult.Succeeded)
            {
                return BadRequest(new
                {
                    message = "User creation failed.",
                    errors = identityResult.Errors.Select(e => new { e.Code, e.Description })
                });
            }

            if (registerRequestDto.Roles != null && registerRequestDto.Roles.Any())
            {
                identityResult = await userManager.AddToRolesAsync(identityUser, registerRequestDto.Roles);
                if (!identityResult.Succeeded)
                {
                    return BadRequest(new
                    {
                        message = "Adding roles failed.",
                        errors = identityResult.Errors.Select(e => new { e.Code, e.Description })
                    });
                }
            }

            try
            {
                var token = await userManager.GenerateEmailConfirmationTokenAsync(identityUser);
                var confirmationLink = $"http://localhost:5173/confirm-email/{identityUser.Id}/{Uri.EscapeDataString(token)}";

                var message = $"<p>Kliknij <a href=\"{confirmationLink}\">tutaj</a> aby potwierdzić rejestrację.</p>";
                await emailSender.SendEmailAsync(identityUser.Email, "Potwierdzenie rejestracji", message);

                return Ok(new { message = "User was registered! Please confirm your email to complete the registration." });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Error sending confirmation email.", error = ex.Message });
            }
        }
        [HttpPost("ResendConfirmation")]
        public async Task<IActionResult> ResendConfirmation([FromBody] ResendConfirmationRequestDto request)
        {
            var user = await userManager.FindByEmailAsync(request.Email);
            if (user == null)
            {
                return BadRequest(new { message = "User with this email does not exist." });
            }

            if (user.EmailConfirmed)
            {
                return BadRequest(new { message = "This account is already confirmed." });
            }

            var token = await userManager.GenerateEmailConfirmationTokenAsync(user);
            var confirmationLink = $"http://localhost:5173/confirm-email/{user.Id}/{Uri.EscapeDataString(token)}";

            var message = $"<p>Kliknij <a href=\"{confirmationLink}\">tutaj</a> aby potwierdzić rejestrację.</p>";
            await emailSender.SendEmailAsync(user.Email, "Potwierdzenie rejestracji", message);

   
            var redirectUrl = $"http://localhost:5173/confirmation-sent?email={Uri.EscapeDataString(request.Email)}&status=success";
            return Ok(new { redirectUrl });
        }


        [HttpGet]
        [Route("ConfirmEmail")]
        public async Task<IActionResult> ConfirmEmail(string userId, string token)
        {
            var user = await userManager.FindByIdAsync(userId);
            if (user == null)
                return BadRequest(new { message = "Invalid User" });

            var result = await userManager.ConfirmEmailAsync(user, token);

            if (result.Succeeded)
            {
                return Ok(new { message = "Email confirmed successfully!" });
            }

            return BadRequest(new { message = "Email confirmation failed" });
        }


        [HttpPost]
        [Route("Login")]
        public async Task<IActionResult> Login([FromBody] LoginRequestDto loginRequestDto)
        {
            var user = await userManager.FindByEmailAsync(loginRequestDto.Username);

            if (user == null)
            {
                return BadRequest(new { message = "Username or password incorrect" });
            }

            if (!user.EmailConfirmed)
            {
                return BadRequest(new { message = "Email not confirmed. Please confirm your email to log in." });
            }
            var checkPasswordResult = await userManager.CheckPasswordAsync(user, loginRequestDto.Password);
            if (!checkPasswordResult)
            {
                return BadRequest(new { message = "Username or password incorrect" });
            }

            var roles = await userManager.GetRolesAsync(user);
            if (roles == null || !roles.Any())
            {
                return BadRequest(new { message = "No roles assigned to user." });
            }
            var jwtToken = tokenRepository.CreateJWTToken(user, roles.ToList());

            var response = new LoginResponseDto
            {
                JwtToken = jwtToken
            };

            return Ok(response);
        }
        [HttpPost]
        [Route("ForgotPassword")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequestDto forgotPasswordRequest)
        {
            var user = await userManager.FindByEmailAsync(forgotPasswordRequest.Email);
            if (user == null || !(await userManager.IsEmailConfirmedAsync(user)))
            {
                return BadRequest(new { message = "Invalid request or email not confirmed." });
            }

            var token = await userManager.GeneratePasswordResetTokenAsync(user);
            var resetLink = $"http://localhost:5173/reset-password/{user.Id}/{Uri.EscapeDataString(token)}";

            var message = $"<p>Kliknij <a href=\"{resetLink}\">tutaj</a>, aby zresetować swoje hasło.</p>";
            await emailSender.SendEmailAsync(user.Email, "Resetowanie hasła", message);

            return Ok(new { message = "Password reset link sent. Please check your email." });
        }

        [HttpPost]
        [Route("ResetPassword")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequestDto resetPasswordRequest)
        {
            var user = await userManager.FindByIdAsync(resetPasswordRequest.UserId);
            if (user == null)
            {
                return BadRequest(new { message = "Invalid user ID." });
            }

            var resetPasswordResult = await userManager.ResetPasswordAsync(user, resetPasswordRequest.Token, resetPasswordRequest.NewPassword);

            if (!resetPasswordResult.Succeeded)
            {
                return BadRequest(new
                {
                    message = "Password reset failed.",
                    errors = resetPasswordResult.Errors.Select(e => new { e.Code, e.Description })
                });
            }

            return Ok(new { message = "Password has been reset successfully." });
        }

    }
}
