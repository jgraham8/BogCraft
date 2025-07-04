using System.Diagnostics;

namespace BogCraft.UI.Services;

public interface IMinecraftServerService
{
    bool IsRunning { get; }
    DateTime? StartTime { get; }
    int PlayerCount { get; }
    List<string> PlayerList { get; }
    bool AutoRestartEnabled { get; set; }
    
    event EventHandler<string>? ConsoleOutput;
    event EventHandler<bool>? StatusChanged;
    event EventHandler<PlayerUpdateEventArgs>? PlayerUpdated;
    
    Task StartServerAsync();
    Task StopServerAsync();
    Task RestartServerAsync();
    Task SendCommandAsync(string command);
    Task SaveWorldAsync();
    Task RefreshPlayersAsync();
}

public class MinecraftServerService : IMinecraftServerService, IDisposable
{
    private Process? _serverProcess;
    private List<string> _playerList = [];
    private readonly ILogService _logService;

    public MinecraftServerService(ILogService logService)
    {
        _logService = logService;
    }

    public bool IsRunning { get; private set; }

    public DateTime? StartTime { get; private set; }

    public int PlayerCount { get; private set; }

    public List<string> PlayerList => _playerList.ToList();
    
    public bool AutoRestartEnabled { get; set; } = true;

    public event EventHandler<string>? ConsoleOutput;
    public event EventHandler<bool>? StatusChanged;
    public event EventHandler<PlayerUpdateEventArgs>? PlayerUpdated;
    
    public async Task StartServerAsync()
    {
        if (IsRunning) return;
        
        try
        {
            _serverProcess = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "cmd",
                    Arguments = "/c run.bat",
                    WorkingDirectory = "D:/NeoBogged",
                    UseShellExecute = false,
                    RedirectStandardInput = true,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true
                }
            };
            
            _serverProcess.OutputDataReceived += OnOutputDataReceived;
            _serverProcess.ErrorDataReceived += OnErrorDataReceived;
            _serverProcess.Exited += OnProcessExited;
            _serverProcess.EnableRaisingEvents = true;
            
            _serverProcess.Start();
            _serverProcess.BeginOutputReadLine();
            _serverProcess.BeginErrorReadLine();
            
            IsRunning = true;
            StartTime = DateTime.Now;
            PlayerCount = 0;
            _playerList.Clear();
            
            var startMessage = "Server starting...";
            _logService.AddLog(startMessage, LogLevel.Information);
            ConsoleOutput?.Invoke(this, startMessage);
            StatusChanged?.Invoke(this, true);
            PlayerUpdated?.Invoke(this, new PlayerUpdateEventArgs { Count = 0, Players = [] });
        }
        catch (Exception ex)
        {
            var errorMessage = $"ERROR: Failed to start server - {ex.Message}";
            _logService.AddLog(errorMessage, LogLevel.Error);
            ConsoleOutput?.Invoke(this, errorMessage);
        }
    }
    
    public async Task StopServerAsync()
    {
        if (_serverProcess?.HasExited == false)
        {
            await _serverProcess.StandardInput.WriteLineAsync("stop");
        }
    }
    
    public async Task RestartServerAsync()
    {
        await StopServerAsync();
        
        // Wait for process to exit, then restart
        _ = Task.Run(async () =>
        {
            await Task.Delay(5000);
            if (!IsRunning)
            {
                await StartServerAsync();
            }
        });
    }
    
    public async Task SendCommandAsync(string command)
    {
        if (_serverProcess?.HasExited == false && !string.IsNullOrWhiteSpace(command))
        {
            await _serverProcess.StandardInput.WriteLineAsync(command);
            var commandMessage = $"COMMAND: {command}";
            _logService.AddLog(commandMessage, LogLevel.Information);
        }
    }
    
    public async Task SaveWorldAsync()
    {
        await SendCommandAsync("save-all");
        var saveMessage = "World save initiated...";
        _logService.AddLog(saveMessage, LogLevel.Information);
        ConsoleOutput?.Invoke(this, saveMessage);
    }
    
    public async Task RefreshPlayersAsync()
    {
        await SendCommandAsync("list");
    }
    
    private void OnOutputDataReceived(object sender, DataReceivedEventArgs e)
    {
        if (string.IsNullOrEmpty(e.Data)) return;
        
        // Add to log service
        _logService.AddLog(e.Data, LogLevel.Information);
        
        ConsoleOutput?.Invoke(this, e.Data);
        ParseServerOutput(e.Data);
    }
    
    private void OnErrorDataReceived(object sender, DataReceivedEventArgs e)
    {
        if (!string.IsNullOrEmpty(e.Data))
        {
            var errorMessage = $"ERROR: {e.Data}";
            _logService.AddLog(errorMessage, LogLevel.Error);
            ConsoleOutput?.Invoke(this, errorMessage);
        }
    }
    
    private void OnProcessExited(object? sender, EventArgs e)
    {
        var exitCode = _serverProcess?.ExitCode ?? -1;
        IsRunning = false;
        StartTime = null;
        PlayerCount = 0;
        _playerList.Clear();
        
        var exitMessage = $"Server stopped with exit code {exitCode}.";
        _logService.AddLog(exitMessage, LogLevel.Warning);
        ConsoleOutput?.Invoke(this, exitMessage);
        StatusChanged?.Invoke(this, false);
        PlayerUpdated?.Invoke(this, new PlayerUpdateEventArgs { Count = 0, Players = [] });
    }
    
    private void ParseServerOutput(string output)
    {
        // Parse player join/leave
        if (output.Contains("joined the game"))
        {
            var match = System.Text.RegularExpressions.Regex.Match(output, @"(\w+) joined the game");
            if (match.Success)
            {
                var playerName = match.Groups[1].Value;
                if (!_playerList.Contains(playerName))
                {
                    _playerList.Add(playerName);
                    PlayerCount = _playerList.Count;
                    PlayerUpdated?.Invoke(this, new PlayerUpdateEventArgs 
                    { 
                        Count = PlayerCount, 
                        Players = _playerList.ToList() 
                    });
                }
            }
        }
        else if (output.Contains("left the game"))
        {
            var match = System.Text.RegularExpressions.Regex.Match(output, @"(\w+) left the game");
            if (match.Success)
            {
                var playerName = match.Groups[1].Value;
                if (_playerList.Remove(playerName))
                {
                    PlayerCount = _playerList.Count;
                    PlayerUpdated?.Invoke(this, new PlayerUpdateEventArgs 
                    { 
                        Count = PlayerCount, 
                        Players = _playerList.ToList() 
                    });
                }
            }
        }
        
        // Parse /list command output
        if (output.Contains("There are ") && output.Contains("players online:"))
        {
            var match = System.Text.RegularExpressions.Regex.Match(output, @"There are (\d+)/\d+ players online:(.*)");
            if (match.Success)
            {
                PlayerCount = int.Parse(match.Groups[1].Value);
                var playersString = match.Groups[2].Value;
                
                if (!string.IsNullOrWhiteSpace(playersString))
                {
                    _playerList = playersString.Split(',')
                        .Select(name => name.Trim())
                        .Where(name => !string.IsNullOrEmpty(name))
                        .ToList();
                }
                else
                {
                    _playerList.Clear();
                }
                
                PlayerUpdated?.Invoke(this, new PlayerUpdateEventArgs 
                { 
                    Count = PlayerCount, 
                    Players = _playerList.ToList() 
                });
            }
        }

        if (!output.Contains("There are 0 of a max of")) return;
        
        PlayerCount = 0;
        _playerList.Clear();
        PlayerUpdated?.Invoke(this, new PlayerUpdateEventArgs { Count = 0, Players = [] });
    }
    
    public void Dispose()
    {
        if (_serverProcess?.HasExited == false)
        {
            _serverProcess.Kill();
        }
        _serverProcess?.Dispose();
    }
}