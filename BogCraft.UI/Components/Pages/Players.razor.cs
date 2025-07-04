using BogCraft.UI.Services;
using BogCraft.UI.Components.Shared;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace BogCraft.UI.Components.Pages;

public partial class Players : ComponentBase
{
    [Inject] private IMinecraftServerService ServerService { get; set; } = null!;
    [Inject] private ISnackbar Snackbar { get; set; } = null!;
    [Inject] private IDialogService DialogService { get; set; } = null!;
    
    private string _broadcastMessage = string.Empty;
    private bool _isRefreshing = false;
    private List<string> _players = [];

    protected override async Task OnInitializedAsync()
    {
        // Subscribe to each event with a handler that has a matching signature.
        ServerService.PlayerUpdated += OnPlayerUpdated;
        ServerService.StatusChanged += OnStatusChanged;
        
        await RefreshPlayers();
    }

    // This handler matches the 'PlayerUpdated' event signature.
    private void OnPlayerUpdated(object? sender, PlayerUpdateEventArgs e)
    {
        UpdateStateAndRender();
    }

    // This handler matches the 'StatusChanged' event signature.
    private void OnStatusChanged(object? sender, bool isRunning)
    {
        UpdateStateAndRender();
    }

    // A single, common method to update state and trigger a re-render.
    private void UpdateStateAndRender()
    {
        _players = ServerService.PlayerList.ToList();
        InvokeAsync(StateHasChanged);
    }

    private async Task RefreshPlayers()
    {
        if (_isRefreshing) return;

        _isRefreshing = true;
        await InvokeAsync(StateHasChanged);

        try
        {
            await ServerService.RefreshPlayersAsync();
            Snackbar.Add("Player list refreshed!", Severity.Info);
        }
        catch (Exception ex)
        {
            Snackbar.Add($"Error refreshing players: {ex.Message}", Severity.Error);
        }
        finally
        {
            _isRefreshing = false;
            // The service event will trigger the final state update, but we call this
            // to ensure the loading spinner is removed even if no player changes occurred.
            UpdateStateAndRender();
        }
    }

    private async Task BroadcastMessage()
    {
        if (string.IsNullOrWhiteSpace(_broadcastMessage)) return;

        await ServerService.SendCommandAsync($"say {_broadcastMessage}");
        _broadcastMessage = string.Empty;
        Snackbar.Add("Broadcast sent!", Severity.Success);
    }

    private async Task KickPlayer(string playerName)
    {
        await ServerService.SendCommandAsync($"kick {playerName}");
        Snackbar.Add($"Kicked player: {playerName}", Severity.Warning);
    }

    private async Task SendPrivateMessage(string playerName)
    {
        var parameters = new DialogParameters<MessageDialog>
        {
            { x => x.PlayerName, playerName }
        };

        var dialog = await DialogService.ShowAsync<MessageDialog>("Send Private Message", parameters);
        var result = await dialog.Result;

        if (result is { Canceled: false, Data: string message })
        {
            await ServerService.SendCommandAsync($"msg {playerName} {message}");
            Snackbar.Add($"Message sent to {playerName}", Severity.Success);
        }
    }

    private async Task SendCommand(string command)
    {
        await ServerService.SendCommandAsync(command);
        Snackbar.Add("Command sent!", Severity.Success);
    }

    public void Dispose()
    {
        // Unsubscribe from the correct handlers to prevent memory leaks.
        ServerService.PlayerUpdated -= OnPlayerUpdated;
        ServerService.StatusChanged -= OnStatusChanged;
    }
}