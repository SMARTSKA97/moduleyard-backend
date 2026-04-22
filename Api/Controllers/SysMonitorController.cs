using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SysMonitorController : ControllerBase
{
    [HttpGet("metrics")]
    public IActionResult GetMetrics()
    {
        // Return mock metrics since ServerMetrics table/entity doesn't exist yet
        var metrics = new[]
        {
            new
            {
                id = Guid.NewGuid().ToString(),
                serverName = "Production-App-1",
                cpuUsagePercentage = 45.2,
                memoryUsageMb = 2048,
                recordedAt = DateTime.UtcNow
            },
            new
            {
                id = Guid.NewGuid().ToString(),
                serverName = "Production-Db-1",
                cpuUsagePercentage = 68.7,
                memoryUsageMb = 8192,
                recordedAt = DateTime.UtcNow
            }
        };

        return Ok(metrics);
    }
}