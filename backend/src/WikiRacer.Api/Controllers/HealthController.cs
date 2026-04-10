using Microsoft.AspNetCore.Mvc;

namespace WikiRacer.Api.Controllers;

[Route("health")]
public sealed class HealthController : ControllerBase
{
    [HttpGet]
    public IActionResult Get()
    {
        return Ok(new
        {
            service = "WikiRacer.Api",
            status = "ok",
            environment = HttpContext.RequestServices.GetRequiredService<IWebHostEnvironment>().EnvironmentName,
            assemblies = new[]
            {
                typeof(WikiRacer.Application.AssemblyReference).Assembly.GetName().Name,
                typeof(WikiRacer.Infrastructure.AssemblyReference).Assembly.GetName().Name,
                typeof(WikiRacer.Contracts.AssemblyReference).Assembly.GetName().Name
            }
        });
    }
}
