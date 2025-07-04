using BogCraft.UI.Components.Shared;
using BogCraft.UI.Services;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace BogCraft.UI.Components.Pages;

public partial class Logs : ComponentBase
{
        private List<ArchivedSession> _archivedSessions = [];
    private bool _loading = true;
    private bool _showLogViewer = false;
    private ArchivedSession? _selectedSession;
    private LogLevel _logFilter = LogLevel.Trace;
    
    private readonly DialogOptions _dialogOptions = new()
    {
        MaxWidth = MaxWidth.ExtraExtraLarge,
        FullWidth = true,
        CloseButton = true,
        BackdropClick = false
    };

    protected override async Task OnInitializedAsync()
    {
        await LoadArchivedSessions();
    }

    private async Task LoadArchivedSessions()
    {
        _loading = true;
        try
        {
            _archivedSessions = await LogService.GetArchivedSessionsAsync();
        }
        finally
        {
            _loading = false;
        }
    }

    private async Task ViewSession(ArchivedSession session)
    {
        _selectedSession = await LogService.GetArchivedSessionAsync(session.SessionId);
        _logFilter = LogLevel.Trace;
        _showLogViewer = true;
    }

    private async Task ExportSession(ArchivedSession? session)
    {
        if (session == null) return;
        
        // This would trigger a file download in a real implementation
        // For now, we'll just show a message
        var parameters = new DialogParameters
        {
            ["ContentText"] = $"Export functionality would download logs for session: {session.SessionId}",
            ["ButtonText"] = "OK",
            ["Color"] = Color.Primary
        };

        var dialog = await DialogService.ShowAsync<ConfirmationDialog>("Export Logs", parameters);
        await dialog.Result;
    }

    private string GetDuration(ArchivedSession session)
    {
        var duration = session.EndTime - session.StartTime;
        return $"{duration.Hours:D2}:{duration.Minutes:D2}:{duration.Seconds:D2}";
    }

    private void SetLogFilter(LogLevel level)
    {
        _logFilter = level;
    }

    private IEnumerable<LogEntry> GetFilteredSessionLogs()
    {
        if (_selectedSession?.Logs == null) return [];
        
        var logs = _selectedSession.Logs;
        
        return _logFilter == LogLevel.Trace ? logs : logs.Where(log => log.Level == _logFilter);
    }

    private string GetLogColor(LogLevel level) => level switch
    {
        LogLevel.Critical => "#ff0000",
        LogLevel.Error => "#ff4444",
        LogLevel.Warning => "#ffaa00",
        LogLevel.Information => "#4488ff",
        LogLevel.Debug => "#888888",
        _ => "#44ff44"
    };
}