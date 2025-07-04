using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace BogCraft.UI.Components.Shared;

public partial class ConfirmationDialog : ComponentBase
{
    [CascadingParameter] public IMudDialogInstance? MudDialog { get; set; }
    [Parameter] public string ContentText { get; set; } = string.Empty;
    [Parameter] public string ButtonText { get; set; } = "Delete";
    [Parameter] public Color Color { get; set; } = Color.Error;

    void Submit() => MudDialog?.Close(DialogResult.Ok(true));
    void Cancel() => MudDialog?.Cancel();
}