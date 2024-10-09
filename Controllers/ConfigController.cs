using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using System.IO;
using System.Threading.Tasks;

public class ConfigController : ControllerBase
{
    private readonly IWebHostEnvironment _env;

    public ConfigController(IWebHostEnvironment env)
    {
        _env = env;
    }

    [HttpGet("api/config")]
    public async Task<IActionResult> GetConfigFile()
    {
        var configPath = Path.Combine(_env.WebRootPath, "config.json");

        if (System.IO.File.Exists(configPath))
        {
            var content = await System.IO.File.ReadAllTextAsync(configPath);
            return Ok(content);
        }
        else
        {
            return NotFound("Config file not found.");
        }
    }
}
