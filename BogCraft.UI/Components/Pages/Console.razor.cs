using BogCraft.UI.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;
using MudBlazor;

namespace BogCraft.UI.Components.Pages;

public partial class Console : ComponentBase
{
    [Inject] private IJSRuntime JSRuntime { get; set; } = null!;
    
    private string _command = string.Empty;
    private LogLevel _activeFilter = LogLevel.Trace;
    private bool _autoScroll = true;
    private List<LogEntry> _filteredLogs = [];
    private bool _shouldScrollToBottom = false;

    protected override async Task OnInitializedAsync()
    {
        ServerService.ConsoleOutput += OnConsoleOutput;
        UpdateFilteredLogs();
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (_shouldScrollToBottom && _autoScroll)
        {
            await ScrollToBottom();
            _shouldScrollToBottom = false;
        }
    }

    private void OnConsoleOutput(object? sender, string message)
    {
        UpdateFilteredLogs();
        _shouldScrollToBottom = true;
        InvokeAsync(StateHasChanged);
    }

    private void UpdateFilteredLogs()
    {
        var allLogs = LogService.CurrentLogs.ToList();
        _filteredLogs = _activeFilter == LogLevel.Trace ? 
            allLogs : 
            allLogs.Where(log => log.Level == _activeFilter).ToList();
    }

    private IEnumerable<LogEntry> GetFilteredLogs()
    {
        return _filteredLogs;
    }

    private void SetFilter(LogLevel level)
    {
        _activeFilter = level;
        UpdateFilteredLogs();
        _shouldScrollToBottom = true;
    }

    private async Task ScrollToBottom()
    {
        try
        {
            await JSRuntime.InvokeVoidAsync("scrollToBottom", "console-container");
        }
        catch (Exception ex)
        {
            // Silently handle JS interop failures
            System.Console.WriteLine($"Scroll failed: {ex.Message}");
        }
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