using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using MudBlazor;

namespace BogCraft.UI.Components.Shared;

public partial class MessageDialog : ComponentBase
{
    [CascadingParameter] public IMudDialogInstance? MudDialog { get; set; }
    [Parameter] public string PlayerName { get; set; } = string.Empty;
    
    private string Message { get; set; } = string.Empty;

    private async Task OnKeyPress(KeyboardEventArgs e)
    {
        if (e is { Key: "Enter", CtrlKey: true } && !string.IsNullOrWhiteSpace(Message))
        {
            await Submit();
        }
    }

    private async Task Submit()
    {
        MudDialog?.Close(DialogResult.Ok(Message));
    }

    private void Cancel()
    {
        MudDialog?.Cancel();
    }
}