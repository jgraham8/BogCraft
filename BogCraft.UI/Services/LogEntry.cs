namespace BogCraft.UI.Services;

public class LogEntry
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Message { get; set; } = string.Empty;
    public LogLevel Level { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.Now;
    public string DisplayTime => Timestamp.ToString("HH:mm:ss");
}