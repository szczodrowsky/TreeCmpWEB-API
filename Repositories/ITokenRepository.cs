using Microsoft.AspNetCore.Identity;

namespace TreeCmpWebAPI.Repositories
{
    public interface ITokenRepository
    {

       string CreateJWTToken(IdentityUser user, List<string> roles);
    }
}
