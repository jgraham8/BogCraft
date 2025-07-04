using BogCraft.UI.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using MudBlazor;

namespace BogCraft.UI.Components.Pages;

public partial class Console : ComponentBase
{
    private string _command = string.Empty;
    private LogLevel _activeFilter = LogLevel.Trace;
    private ElementReference consoleElement;
    private bool _autoScroll = true;
    private List<LogEntry> _logSnapshot = [];

    protected override async Task OnInitializedAsync()
    {
        ServerService.ConsoleOutput += OnConsoleOutput;
        UpdateLogSnapshot();
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (_autoScroll && consoleElement.Context != null)
        {
            await consoleElement.FocusAsync();
        }
    }

    private void OnConsoleOutput(object? sender, string message)
    {
        UpdateLogSnapshot();
        InvokeAsync(StateHasChanged);
    }

    private void UpdateLogSnapshot()
    {
        _logSnapshot = LogService.CurrentLogs.ToList();
    }

    private IEnumerable<LogEntry> GetFilteredLogs()
    {
        return _activeFilter == LogLevel.Trace ? _logSnapshot : _logSnapshot.Where(log => log.Level == _activeFilter);
    }

    private void SetFilter(LogLevel level)
    {
        _activeFilter = level;
        UpdateLogSnapshot();
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

    private async Task SendCommand()
    {
        if (!string.IsNullOrWhiteSpace(_command))
        {
            await ServerService.SendCommandAsync(_command);
            _command = string.Empty;
            Snackbar.Add("Command sent!", Severity.Success);
        }
    }

    private async Task OnKeyPress(KeyboardEventArgs e)
    {
        if (e.Key == "Enter")
        {
            await SendCommand();
        }
    }

    public async ValueTask DisposeAsync()
    {
        ServerService.ConsoleOutput -= OnConsoleOutput;
    }
}