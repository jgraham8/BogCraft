using BogCraft.UI.Services;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace BogCraft.UI.Components.Shared;

public partial class ServerStatusCard : ComponentBase
{
    [Parameter] public IMinecraftServerService ServerService { get; set; } = null!;
    
    private Timer? _uptimeTimer;

    protected override void OnInitialized()
    {
        ServerService.StatusChanged += OnStatusChanged;
        _uptimeTimer = new Timer(UpdateUptime, null, TimeSpan.Zero, TimeSpan.FromSeconds(1));
    }

    private void OnStatusChanged(object? sender, bool isRunning)
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

    public void Dispose()
    {
        ServerService.StatusChanged -= OnStatusChanged;
        _uptimeTimer?.Dispose();
    }
}