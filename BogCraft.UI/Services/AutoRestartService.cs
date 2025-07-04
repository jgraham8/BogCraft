namespace BogCraft.UI.Services;

public class AutoRestartService(
    IMinecraftServerService serverService,
    ILogService logService,
    ILogger<AutoRestartService> logger)
    : BackgroundService
{
    private bool _restartScheduled = false;
    private readonly TimeSpan _restartTime = new(0, 0, 0); // 00:00:00 UTC

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await CheckForScheduledRestart();
                await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error in auto-restart service");
            }
        }
    }

    private async Task CheckForScheduledRestart()
    {
        if (!serverService.AutoRestartEnabled || !serverService.IsRunning)
        {
            _restartScheduled = false;
            return;
        }

        var now = DateTime.UtcNow;
        var currentTime = now.TimeOfDay;
        
        // Check if we're within 5 minutes of restart time
        var timeDifference = _restartTime - currentTime;
        if (timeDifference.TotalMinutes is <= 5 and > 0 && !_restartScheduled)
        {
            _restartScheduled = true;
            await ScheduleRestart(timeDifference);
        }
        
        // Reset schedule flag after restart time has passed
        if (currentTime > _restartTime.Add(TimeSpan.FromMinutes(10)))
        {
            _restartScheduled = false;
        }
    }

    private async Task ScheduleRestart(TimeSpan timeUntilRestart)
    {
        logger.LogInformation("Scheduling server restart in {Minutes} minutes", timeUntilRestart.TotalMinutes);
        logService.AddLog($"SCHEDULED RESTART: Server will restart in {timeUntilRestart.TotalMinutes:F0} minutes for daily maintenance", LogLevel.Warning);

        // Send warnings at 5, 3, and 1 minutes
        var warnings = new[]
        {
            TimeSpan.FromMinutes(5),
            TimeSpan.FromMinutes(3),
            TimeSpan.FromMinutes(1)
        };

        foreach (var warning in warnings)
        {
            if (timeUntilRestart < warning) continue;
            
            var delay = timeUntilRestart - warning;
            _ = Task.Run(async () =>
            {
                await Task.Delay(delay);
                await serverService.SendCommandAsync($"say [SERVER] Daily restart in {warning.TotalMinutes} minutes! Save your progress.");
            });
        }

        // Schedule the actual restart
        _ = Task.Run(async () =>
        {
            await Task.Delay(timeUntilRestart);
            await PerformRestart();
        });
    }

    private async Task PerformRestart()
    {
        logger.LogInformation("Performing scheduled server restart");
        logService.AddLog("SYSTEM: Performing scheduled daily restart...", LogLevel.Warning);

        try
        {
            await serverService.SendCommandAsync("say [SERVER] Restarting now! Server will be back in 1-2 minutes.");
            await Task.Delay(TimeSpan.FromSeconds(3));
            
            // Archive current session before restart
            await logService.ArchiveSessionAsync();
            
            await serverService.SendCommandAsync("save-all");
            await Task.Delay(TimeSpan.FromSeconds(3));
            
            await serverService.StopServerAsync();
            
            // Wait for server to stop, then restart
            await Task.Delay(TimeSpan.FromSeconds(10));
            
            if (!serverService.IsRunning)
            {
                await serverService.StartServerAsync();
                logService.AddLog("SYSTEM: Scheduled restart completed", LogLevel.Information);
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error during scheduled restart");
            logService.AddLog($"ERROR: Scheduled restart failed - {ex.Message}", LogLevel.Error);
        }
        finally
        {
            _restartScheduled = false;
        }
    }
}