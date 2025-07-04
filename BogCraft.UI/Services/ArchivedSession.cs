namespace BogCraft.UI.Services;

public class ArchivedSession
{
    public string SessionId { get; set; } = string.Empty;
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public int LogCount { get; set; }
    public List<LogEntry> Logs { get; set; } = [];
}