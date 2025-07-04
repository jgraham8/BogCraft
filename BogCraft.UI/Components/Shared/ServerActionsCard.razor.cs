using BogCraft.UI.Services;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace BogCraft.UI.Components.Shared;

public partial class ServerActionsCard : ComponentBase
{
    [Parameter] public IMinecraftServerService ServerService { get; set; } = null!;

    private async Task StartServer()
    {
        await ServerService.StartServerAsync();
        Snackbar.Add("Starting server...", Severity.Info);
    }

    private async Task StopServer(bool save = true)
    {
        if (save)
        {
            await ServerService.SaveWorldAsync();
            await Task.Delay(2000); // Wait for save to complete
            Snackbar.Add("Saving world and stopping server...", Severity.Warning);
        }
        else
        {
            Snackbar.Add("Force stopping server...", Severity.Error);
        }
        
        await ServerService.StopServerAsync();
    }

    private async Task RestartServer(bool save = true)
    {
        if (save)
        {
            await ServerService.SaveWorldAsync();
            await Task.Delay(2000); // Wait for save to complete
            Snackbar.Add("Saving world and restarting server...", Severity.Warning);
        }
        else
        {
            Snackbar.Add("Force restarting server...", Severity.Warning);
        }
        
        await ServerService.RestartServerAsync();
    }

    private async Task SaveWorld()
    {
        await ServerService.SaveWorldAsync();
        Snackbar.Add("Saving world...", Severity.Info);
    }
}