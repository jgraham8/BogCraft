using BogCraft.UI.Services;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace BogCraft.UI.Components.Pages;

public partial class Home : ComponentBase
{
    [Inject] private IMinecraftServerService ServerService { get; set; } = null!;
    [Inject] private ILogService LogService { get; set; } = null!;
    [Inject] private ISnackbar Snackbar { get; set; } = null!;
    
    private Timer? _uptimeTimer;

    protected override void OnInitialized()
    {
        ServerService.ConsoleOutput += OnConsoleOutput;
        ServerService.StatusChanged += OnStatusChanged;
        ServerService.PlayerUpdated += OnPlayerUpdated;
        _uptimeTimer = new Timer(UpdateUptime, null, TimeSpan.Zero, TimeSpan.FromSeconds(1));
    }

    private void OnConsoleOutput(object? sender, string message)
    {
        InvokeAsync(StateHasChanged);
    }

    private void OnStatusChanged(object? sender, bool isRunning)
    {
        InvokeAsync(StateHasChanged);
    }

    private void OnPlayerUpdated(object? sender, PlayerUpdateEventArgs e)
    {
        InvokeAsync(StateHasChanged);
    }

    private void UpdateUptime(object? state)
    {
        if (ServerService.IsRunning)
        {
            InvokeAsync(StateHasChanged);
        }
    }

    private async Task StartServer()
    {
        await ServerService.StartServerAsync();
    }

    private async Task StopServer(bool save = true)
    {
        if (save)
        {
            await ServerService.SaveWorldAsync();
            await Task.Delay(2000);
        }
        await ServerService.StopServerAsync();
    }

    private async Task RestartServer(bool save = true)
    {
        if (save)
        {
            await ServerService.SaveWorldAsync();
            await Task.Delay(2000);
        }
        await ServerService.RestartServerAsync();
    }

    private async Task SaveWorld()
    {
        await ServerService.SaveWorldAsync();
    }

    private async Task ClearLogs()
    {
        LogService.ClearLogs();
    }

    private async Task ArchiveSession()
    {
        await LogService.ArchiveSessionAsync();
        Snackbar.Add("Session archived successfully!", Severity.Success);
    }

    private async Task RefreshPlayers()
    {
        await ServerService.RefreshPlayersAsync();
        Snackbar.Add("Refreshing player list...", Severity.Info);
    }

    private async Task ExportLogs()
    {
        Snackbar.Add("Exporting logs...", Severity.Info);
        // Export functionality would go here
    }

    private string GetLocalIP()
    {
        try
        {
            var host = System.Net.Dns.GetHostEntry(System.Net.Dns.GetHostName());
            var localIP = host.AddressList
                .FirstOrDefault(ip => ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork && 
                                     !System.Net.IPAddress.IsLoopback(ip));
            return localIP?.ToString() ?? "localhost";
        }
        catch
        {
            return "localhost";
        }
    }

    private Severity GetSeverity()
    {
        return ServerService.IsRunning ? Severity.Success : Severity.Error;
    }

    private string GetStatusIcon()
    {
        return ServerService.IsRunning ? Icons.Material.Filled.CheckCircle : Icons.Material.Filled.Cancel;
    }

    private string GetUptime()
    {
        if (!ServerService.IsRunning || ServerService.StartTime == null)
            return "Not running";

        var uptime = DateTime.Now - ServerService.StartTime.Value;
        return $"{uptime.Hours:D2}:{uptime.Minutes:D2}:{uptime.Seconds:D2}";
    }

    private string GetMemoryUsage()
    {
        var memoryMB = LogService.CurrentLogCount * 0.1; // Rough estimate
        return $"{memoryMB:F1} MB";
    }

    private string GetLogColor(LogLevel level) => level switch
    {
        LogLevel.Error => "#ff4444",
        LogLevel.Warning => "#ffaa00",
        LogLevel.Information => "#4488ff",
        LogLevel.Debug => "#888888",
        _ => "#44ff44"
    };

    public void Dispose()
    {
        ServerService.ConsoleOutput -= OnConsoleOutput;
        ServerService.StatusChanged -= OnStatusChanged;
        ServerService.PlayerUpdated -= OnPlayerUpdated;
        _uptimeTimer?.Dispose();
    }
}