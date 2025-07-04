using Microsoft.AspNetCore.SignalR;
using BogCraft.UI.Services;

namespace BogCraft.UI.Hubs;

public class ServerConsoleHub(IMinecraftServerService serverService, ILogService logService)
    : Hub
{
    public override async Task OnConnectedAsync()
    {
        // Send current status to new client
        await Clients.Caller.SendAsync("StatusUpdate", new
        {
            IsRunning = serverService.IsRunning,
            StartTime = serverService.StartTime,
            PlayerCount = serverService.PlayerCount,
            Players = serverService.PlayerList
        });
        
        // Send current logs
        await Clients.Caller.SendAsync("LogHistory", new
        {
            SessionId = logService.CurrentSessionId,
            Logs = logService.CurrentLogs.TakeLast(100).ToList() // Send last 100 logs
        });
        
        await base.OnConnectedAsync();
    }
    
    public async Task SendCommand(string command)
    {
        await serverService.SendCommandAsync(command);
    }
}