using MudBlazor;

namespace BogCraft.UI.Components.Layout;
public partial class MainLayout
{
    private bool _drawerOpen = true;
    private bool _isDarkMode = true;
    private MudThemeProvider? _mudThemeProvider;

    protected override async Task OnInitializedAsync()
    {
        // Load theme from persistent settings first
        await ThemeService.LoadThemeAsync();
        _isDarkMode = ThemeService.IsDarkMode;
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender && _mudThemeProvider != null)
        {
            await _mudThemeProvider.WatchSystemPreference(OnSystemPreferenceChanged);
        }
    }

    private Task OnSystemPreferenceChanged(bool newValue)
    {
        _isDarkMode = newValue;
        StateHasChanged();
        return Task.CompletedTask;
    }

    private async Task ToggleTheme()
    {
        await ThemeService.ToggleThemeAsync();
        _isDarkMode = ThemeService.IsDarkMode;
    }

    private void ToggleDrawer()
    {
        _drawerOpen = !_drawerOpen;
    }
}