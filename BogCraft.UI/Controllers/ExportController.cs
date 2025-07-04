using Microsoft.AspNetCore.Mvc;
using BogCraft.UI.Services;

namespace BogCraft.UI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ExportController(ILogService logService) : ControllerBase
{
    [HttpGet("logs/{sessionId}")]
    public async Task<IActionResult> ExportLogs(string sessionId, [FromQuery] string format = "txt")
    {
        try
        {
            await logService.ExportLogsAsync(sessionId, format);
            var filePath = logService.GetExportFilePath(sessionId, format);
            
            if (filePath == null || !System.IO.File.Exists(filePath))
            {
                return NotFound("Export file not found");
            }

            var fileBytes = await System.IO.File.ReadAllBytesAsync(filePath);
            var fileName = $"{sessionId}_logs.{format}";
            var contentType = format.Equals("json", StringComparison.CurrentCultureIgnoreCase) ? "application/json" : "text/plain";

            return File(fileBytes, contentType, fileName);
        }
        catch (Exception ex)
        {
            return BadRequest($"Failed to export logs: {ex.Message}");
        }
    }
}