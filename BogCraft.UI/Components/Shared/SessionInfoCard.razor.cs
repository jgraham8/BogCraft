using BogCraft.UI.Services;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace BogCraft.UI.Components.Shared;

public partial class SessionInfoCard : ComponentBase
{
    [Parameter] public ILogService LogService { get; set; } = null!;

    private async Task ArchiveSession()
    {
        var parameters = new DialogParameters
        {
            ["ContentText"] = "Are you sure you want to archive the current session? This will save all current logs and start a fresh session.",
            ["ButtonText"] = "Archive",
            ["Color"] = Color.Secondary
        };

        var dialog = await DialogService.ShowAsync<ConfirmationDialog>("Archive Session", parameters);
        var result = await dialog.Result;

        if (result is not { Canceled: true })
        {
            await LogService.ArchiveSessionAsync();
            Snackbar.Add("Session archived successfully!", Severity.Success);
        }
    }

    private async Task ClearLogs()
    {
        var parameters = new DialogParameters
        {
            ["ContentText"] = "Are you sure you want to clear all current logs? This action cannot be undone.",
            ["ButtonText"] = "Clear",
            ["Color"] = Color.Warning
        };

        var dialog = await DialogService.ShowAsync<ConfirmationDialog>("Clear Logs", parameters);
        var result = await dialog.Result;

        if (result is not { Canceled: true })
        {
            LogService.ClearLogs();
            Snackbar.Add("Logs cleared!", Severity.Warning);
        }
    }

    private async Task ExportLogs()
    {
        // This would typically trigger a download
        Snackbar.Add("Exporting logs...", Severity.Info);
        // Implementation would depend on how you want to handle file downloads
    }

    private string GetMemoryUsage()
    {
        var memoryMB = LogService.CurrentLogCount * 0.1; // Rough estimate
        return $"{memoryMB:F1} MB";
    }
}