using System.Text.Json;

namespace BogCraft.UI.Services;

public static class TestDataSeeder
{
    public static async Task SeedTestDataAsync(ILogService logService)
    {
        // Create some test archived sessions if none exist
        var existingSessions = await logService.GetArchivedSessionsAsync();
        
        if (existingSessions.Count == 0)
        {
            await CreateTestSession("session_2024-01-01_10-30-00", new DateTime(2024, 1, 1, 10, 30, 0));
            await CreateTestSession("session_2024-01-02_14-15-30", new DateTime(2024, 1, 2, 14, 15, 30));
            await CreateTestSession("session_2024-01-03_09-45-15", new DateTime(2024, 1, 3, 9, 45, 15));
        }
    }
    
    private static async Task CreateTestSession(string sessionId, DateTime startTime)
    {
        var testLogs = new List<LogEntry>
        {
            new() { Message = "Server starting...", Level = LogLevel.Information, Timestamp = startTime },
            new() { Message = "Loading world data...", Level = LogLevel.Information, Timestamp = startTime.AddSeconds(5) },
            new() { Message = "PlayerJoe joined the game", Level = LogLevel.Information, Timestamp = startTime.AddMinutes(2) },
            new() { Message = "Warning: Server overloaded!", Level = LogLevel.Warning, Timestamp = startTime.AddMinutes(5) },
            new() { Message = "PlayerJoe left the game", Level = LogLevel.Information, Timestamp = startTime.AddMinutes(30) },
            new() { Message = "World saved", Level = LogLevel.Information, Timestamp = startTime.AddMinutes(35) }
        };
        
        var session = new ArchivedSession
        {
            SessionId = sessionId,
            StartTime = startTime,
            EndTime = startTime.AddMinutes(40),
            LogCount = testLogs.Count,
            Logs = testLogs
        };
        
        var logsDirectory = "session_logs";
        Directory.CreateDirectory(logsDirectory);
        
        var fileName = Path.Combine(logsDirectory, $"{sessionId}.json");
        var json = JsonSerializer.Serialize(session, new JsonSerializerOptions { WriteIndented = true });
        
        await File.WriteAllTextAsync(fileName, json);
    }
}