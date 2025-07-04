using System.Text.Json;

namespace BogCraft.UI.Services;

public interface ISettingsService
{
    AppSettings Settings { get; }
    Task LoadSettingsAsync();
    Task SaveSettingsAsync();
    Task UpdateSettingAsync<T>(string key, T value);
}

public class AppSettings
{
    public bool AutoRestartEnabled { get; set; } = true;
    public bool AutoScroll { get; set; } = true;
    public int MaxLogs { get; set; } = 1000;
    public string ServerPath { get; set; } = "D:/NeoBogged";
    public bool DarkMode { get; set; } = true;
}

public class SettingsService : ISettingsService
{
    private readonly string _settingsPath = Path.Combine("Data", "settings.json");

    public AppSettings Settings { get; private set; } = new();

    public SettingsService()
    {
        Directory.CreateDirectory(Path.GetDirectoryName(_settingsPath)!);
    }

    public async Task LoadSettingsAsync()
    {
        try
        {
            if (File.Exists(_settingsPath))
            {
                var json = await File.ReadAllTextAsync(_settingsPath);
                var settings = JsonSerializer.Deserialize<AppSettings>(json);
                if (settings != null)
                {
                    Settings = settings;
                }
            }
        }
        catch (Exception ex)
        {
            // Log error but continue with defaults
            Console.WriteLine($"Error loading settings: {ex.Message}");
        }
    }

    public async Task SaveSettingsAsync()
    {
        try
        {
            var json = JsonSerializer.Serialize(Settings, new JsonSerializerOptions 
            { 
                WriteIndented = true 
            });
            await File.WriteAllTextAsync(_settingsPath, json);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error saving settings: {ex.Message}");
        }
    }

    public async Task UpdateSettingAsync<T>(string key, T value)
    {
        var property = typeof(AppSettings).GetProperty(key);
        if (property != null && property.CanWrite)
        {
            property.SetValue(Settings, value);
            await SaveSettingsAsync();
        }
    }
}