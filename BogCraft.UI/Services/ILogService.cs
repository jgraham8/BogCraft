using System.Text;
using System.Text.Json;

namespace BogCraft.UI.Services;

public interface ILogService
{
    string CurrentSessionId { get; }
    IReadOnlyList<LogEntry> CurrentLogs { get; }
    int CurrentLogCount { get; }
    
    void AddLog(string message, LogLevel level = LogLevel.Information);
    void ClearLogs();
    Task ArchiveSessionAsync();
    Task<List<ArchivedSession>> GetArchivedSessionsAsync();
    Task<ArchivedSession?> GetArchivedSessionAsync(string sessionId);
    Task ExportLogsAsync(string sessionId, string format = "txt");
    string? GetExportFilePath(string sessionId, string format = "txt");
}

public class LogService : ILogService
{
    private readonly List<LogEntry> _currentLogs = [];
    private readonly object _lockObject = new();
    private const string LogsDirectory = "session_logs";
    private const int MaxCurrentLogs = 1000;

    public string CurrentSessionId { get; private set; } = GenerateSessionId();
    
    public IReadOnlyList<LogEntry> CurrentLogs 
    { 
        get 
        { 
            lock (_lockObject) 
            { 
                return _currentLogs.ToList().AsReadOnly(); 
            } 
        } 
    }
    
    public int CurrentLogCount 
    { 
        get 
        { 
            lock (_lockObject) 
            { 
                return _currentLogs.Count; 
            } 
        } 
    }
    
    public LogService()
    {
        Directory.CreateDirectory(LogsDirectory);
        Directory.CreateDirectory(Path.Combine("Data", "exports"));
    }
    
    public void AddLog(string message, LogLevel level = LogLevel.Information)
    {
        var logEntry = new LogEntry
        {
            Message = message,
            Level = level,
            Timestamp = DateTime.Now
        };
        
        lock (_lockObject)
        {
            _currentLogs.Add(logEntry);
            
            // Trim logs if exceeding maximum
            if (_currentLogs.Count > MaxCurrentLogs)
            {
                _currentLogs.RemoveRange(0, _currentLogs.Count - MaxCurrentLogs);
            }
        }
    }
    
    public void ClearLogs()
    {
        lock (_lockObject)
        {
            _currentLogs.Clear();
        }
    }
    
    public async Task ArchiveSessionAsync()
    {
        List<LogEntry> logsToArchive;
        
        lock (_lockObject)
        {
            if (_currentLogs.Count == 0) return;
            logsToArchive = _currentLogs.ToList();
        }
        
        var archivedSession = new ArchivedSession
        {
            SessionId = CurrentSessionId,
            StartTime = logsToArchive.First().Timestamp,
            EndTime = DateTime.Now,
            LogCount = logsToArchive.Count,
            Logs = logsToArchive
        };
        
        var fileName = Path.Combine(LogsDirectory, $"{CurrentSessionId}.json");
        var json = JsonSerializer.Serialize(archivedSession, new JsonSerializerOptions 
        { 
            WriteIndented = true 
        });
        
        await File.WriteAllTextAsync(fileName, json);
        
        // Start new session
        lock (_lockObject)
        {
            _currentLogs.Clear();
            CurrentSessionId = GenerateSessionId();
        }
    }
    
    public async Task<List<ArchivedSession>> GetArchivedSessionsAsync()
    {
        var sessions = new List<ArchivedSession>();
        
        if (!Directory.Exists(LogsDirectory))
            return sessions;
        
        var files = Directory.GetFiles(LogsDirectory, "*.json");
        
        foreach (var file in files)
        {
            try
            {
                var json = await File.ReadAllTextAsync(file);
                var session = JsonSerializer.Deserialize<ArchivedSession>(json);
                
                if (session == null) continue;
                
                // Don't load full logs for list view
                session.Logs.Clear();
                sessions.Add(session);
            }
            catch (Exception ex)
            {
                // Log error but continue
                Console.WriteLine($"Error reading session file {file}: {ex.Message}");
            }
        }
        
        return sessions.OrderByDescending(s => s.StartTime).ToList();
    }
    
    public async Task<ArchivedSession?> GetArchivedSessionAsync(string sessionId)
    {
        var fileName = Path.Combine(LogsDirectory, $"{sessionId}.json");
        
        if (!File.Exists(fileName))
            return null;
        
        try
        {
            var json = await File.ReadAllTextAsync(fileName);
            return JsonSerializer.Deserialize<ArchivedSession>(json);
        }
        catch
        {
            return null;
        }
    }
    
    public async Task ExportLogsAsync(string sessionId, string format = "txt")
    {
        ArchivedSession? session;
        
        if (sessionId == CurrentSessionId)
        {
            List<LogEntry> currentLogsCopy;
            lock (_lockObject)
            {
                currentLogsCopy = _currentLogs.ToList();
            }
            
            session = new ArchivedSession
            {
                SessionId = CurrentSessionId,
                StartTime = currentLogsCopy.Any() ? currentLogsCopy.First().Timestamp : DateTime.Now,
                EndTime = DateTime.Now,
                LogCount = currentLogsCopy.Count,
                Logs = currentLogsCopy
            };
        }
        else
        {
            session = await GetArchivedSessionAsync(sessionId);
        }
        
        if (session == null) return;

        var fileName = $"{session.SessionId}_logs.{format}";
        var exportContent = string.Empty;
        
        if (format.ToLower() == "json")
        {
            exportContent = JsonSerializer.Serialize(session, new JsonSerializerOptions 
            { 
                WriteIndented = true 
            });
        }
        else
        {
            var sb = new StringBuilder();
            sb.AppendLine($"# BogCraft Server Logs - {session.SessionId}");
            sb.AppendLine($"# Exported: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
            sb.AppendLine($"# Session Start: {session.StartTime:yyyy-MM-dd HH:mm:ss}");
            sb.AppendLine($"# Session End: {session.EndTime:yyyy-MM-dd HH:mm:ss}");
            sb.AppendLine($"# Total Logs: {session.LogCount}");
            sb.AppendLine();
            
            foreach (var log in session.Logs)
            {
                sb.AppendLine($"[{log.DisplayTime}] [{log.Level}] {log.Message}");
            }
            exportContent = sb.ToString();
        }
        
        // Save to exports directory
        var exportsDir = Path.Combine("Data", "exports");
        Directory.CreateDirectory(exportsDir);
        var filePath = Path.Combine(exportsDir, fileName);
        
        await File.WriteAllTextAsync(filePath, exportContent);
    }

    public string? GetExportFilePath(string sessionId, string format = "txt")
    {
        var fileName = $"{sessionId}_logs.{format}";
        var filePath = Path.Combine("Data", "exports", fileName);
        return File.Exists(filePath) ? filePath : null;
    }
    
    private static string GenerateSessionId()
    {
        return $"session_{DateTime.Now:yyyy-MM-dd_HH-mm-ss}";
    }
}