using BogCraft.UI.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using MudBlazor;

namespace BogCraft.UI.Components.Shared;

public partial class SessionInfoCard : ComponentBase
{
    [Parameter] public ILogService LogService { get; set; } = null!;
    [Inject] private ISnackbar Snackbar { get; set; } = null!;
    [Inject] private IDialogService DialogService { get; set; } = null!;
    [Inject] private IJSRuntime JSRuntime { get; set; } = null!;

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
        try
        {
            await LogService.ExportLogsAsync(LogService.CurrentSessionId);
            
            // Trigger download
            var downloadUrl = $"/api/export/logs/{LogService.CurrentSessionId}?format=txt";
            await JSRuntime.InvokeVoidAsync("open", downloadUrl, "_blank");
            
            Snackbar.Add("Logs exported and download started!", Severity.Success);
        }
        catch (Exception ex)
        {
            Snackbar.Add($"Export failed: {ex.Message}", Severity.Error);
        }
    }

    private string GetMemoryUsage()
    {
        var memoryMB = LogService.CurrentLogCount * 0.1; // Rough estimate
        return $"{memoryMB:F1} MB";
    }
}