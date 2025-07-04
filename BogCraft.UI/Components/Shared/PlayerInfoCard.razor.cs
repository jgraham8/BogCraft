using BogCraft.UI.Services;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace BogCraft.UI.Components.Shared;

public partial class PlayerInfoCard : ComponentBase
{
    [Parameter] public IMinecraftServerService ServerService { get; set; } = null!;

    protected override void OnInitialized()
    {
        ServerService.PlayerUpdated += OnPlayerUpdated;
        ServerService.StatusChanged += OnStatusChanged;
    }

    private void OnPlayerUpdated(object? sender, PlayerUpdateEventArgs e)
    {
        InvokeAsync(StateHasChanged);
    }

    private void OnStatusChanged(object? sender, bool isRunning)
    {
        InvokeAsync(StateHasChanged);
    }

    private async Task RefreshPlayers()
    {
        await ServerService.RefreshPlayersAsync();
        Snackbar.Add("Refreshing player list...", Severity.Info);
    }

    public void Dispose()
    {
        ServerService.PlayerUpdated -= OnPlayerUpdated;
        ServerService.StatusChanged -= OnStatusChanged;
    }
}