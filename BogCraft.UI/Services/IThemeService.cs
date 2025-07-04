using Microsoft.JSInterop;
using MudBlazor;

namespace BogCraft.UI.Services;

public interface IThemeService 
{
    MudTheme CurrentTheme { get; }
    bool IsDarkMode { get; }
    Task ToggleThemeAsync();
    Task LoadThemeAsync();
    Task SetThemeAsync(bool isDarkMode);
}

public class ThemeService(IJSRuntime jsRuntime, ISettingsService settingsService) : IThemeService
{
    public MudTheme CurrentTheme => IsDarkMode ? DarkTheme : LightTheme;
    public bool IsDarkMode { get; private set; } = true;

    public async Task ToggleThemeAsync()
    {
        IsDarkMode = !IsDarkMode;
        await SaveThemeAsync();
    }

    public async Task SetThemeAsync(bool isDarkMode)
    {
        IsDarkMode = isDarkMode;
        await SaveThemeAsync();
    }
    
    public async Task LoadThemeAsync()
    {
        // Load from settings service instead of localStorage
        await settingsService.LoadSettingsAsync();
        IsDarkMode = settingsService.Settings.DarkMode;
    }
    
    private async Task SaveThemeAsync()
    {
        try
        {
            // Save to both localStorage (for immediate UI updates) and settings file (for persistence)
            await jsRuntime.InvokeVoidAsync("localStorage.setItem", "theme", IsDarkMode ? "dark" : "light");
            await settingsService.UpdateSettingAsync(nameof(AppSettings.DarkMode), IsDarkMode);
        }
        catch
        {
            // If localStorage fails, still save to settings file
            await settingsService.UpdateSettingAsync(nameof(AppSettings.DarkMode), IsDarkMode);
        }
    }
    
    private static readonly MudTheme DarkTheme = new()
    {
        PaletteLight = new PaletteLight(),
        PaletteDark = new PaletteDark
        {
            Primary = Colors.Blue.Lighten1,
            Secondary = Colors.DeepPurple.Accent2,
            AppbarBackground = Colors.BlueGray.Darken4,
            Background = Colors.Gray.Darken4,
            BackgroundGray = Colors.Gray.Darken3,
            Surface = Colors.Gray.Darken3,
            DrawerBackground = Colors.Gray.Darken4,
            DrawerText = Colors.Shades.White,
            DrawerIcon = Colors.Shades.White,
            Success = Colors.Green.Accent3,
            Warning = Colors.Orange.Accent3,
            Error = Colors.Red.Accent3,
            Info = Colors.Blue.Accent3
        }
    };
    
    private static readonly MudTheme LightTheme = new()
    {
        PaletteLight = new PaletteLight
        {
            Primary = Colors.Blue.Default,
            Secondary = Colors.DeepPurple.Default,
            AppbarBackground = Colors.Blue.Default,
            Background = Colors.Gray.Lighten5,
            BackgroundGray = Colors.Gray.Lighten4,
            Surface = Colors.Shades.White,
            DrawerBackground = Colors.Shades.White,
            DrawerText = Colors.Shades.Black,
            DrawerIcon = Colors.Shades.Black,
            Success = Colors.Green.Default,
            Warning = Colors.Orange.Default,
            Error = Colors.Red.Default,
            Info = Colors.Blue.Default
        },
        PaletteDark = new PaletteDark()
    };
}