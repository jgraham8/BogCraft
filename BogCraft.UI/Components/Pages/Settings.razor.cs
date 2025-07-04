using BogCraft.UI.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using MudBlazor;

namespace BogCraft.UI.Components.Pages;

public partial class Settings : ComponentBase
{
    private bool _autoRestartEnabled;
    private bool _autoScroll;
    private int _maxLogs;
    private string _serverPath = string.Empty;
    private string _localAddress = string.Empty;
    private string _networkAddress = string.Empty;
    private bool _settingsLoaded = false;

    protected override async Task OnInitializedAsync()
    {
        await SettingsService.LoadSettingsAsync();
        
        // Load settings from service
        _autoRestartEnabled = SettingsService.Settings.AutoRestartEnabled;
        _autoScroll = SettingsService.Settings.AutoScroll;
        _maxLogs = SettingsService.Settings.MaxLogs;
        _serverPath = SettingsService.Settings.ServerPath;
        
        // Apply loaded settings to services
        ServerService.AutoRestartEnabled = _autoRestartEnabled;
        
        // Get network addresses
        _localAddress = GetLocalAddress();
        _networkAddress = GetNetworkAddress();
        
        _settingsLoaded = true;
    }

    private async Task OnAutoRestartChanged(bool value)
    {
        ServerService.AutoRestartEnabled = value;
        await SettingsService.UpdateSettingAsync(nameof(AppSettings.AutoRestartEnabled), value);
        Snackbar.Add($"Auto-restart {(value ? "enabled" : "disabled")}", Severity.Success);
    }

    private async Task OnAutoScrollChanged(bool value)
    {
        await SettingsService.UpdateSettingAsync(nameof(AppSettings.AutoScroll), value);
        Snackbar.Add($"Auto-scroll {(value ? "enabled" : "disabled")}", Severity.Info);
    }

    private async Task OnMaxLogsChanged(int value)
    {
        await SettingsService.UpdateSettingAsync(nameof(AppSettings.MaxLogs), value);
        Snackbar.Add($"Max logs set to {value}", Severity.Info);
    }

    private async Task OnServerPathChanged()
    {
        await SettingsService.UpdateSettingAsync(nameof(AppSettings.ServerPath), _serverPath);
        Snackbar.Add("Server path updated", Severity.Info);
    }

    private async Task ManualSave()
    {
        await SettingsService.SaveSettingsAsync();
        Snackbar.Add("All settings saved successfully!", Severity.Success);
    }

    private async Task CopyToClipboard(string text)
    {
        try
        {
            await JSRuntime.InvokeVoidAsync("navigator.clipboard.writeText", text);
            Snackbar.Add("Address copied to clipboard!", Severity.Success);
        }
        catch
        {
            Snackbar.Add("Failed to copy to clipboard", Severity.Error);
        }
    }

    private string GetLocalAddress()
    {
        return "http://localhost:5091";
    }

    private string GetNetworkAddress()
    {
        try
        {
            var host = System.Net.Dns.GetHostEntry(System.Net.Dns.GetHostName());
            var localIP = host.AddressList
                .FirstOrDefault(ip => ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork && 
                                     !System.Net.IPAddress.IsLoopback(ip));
            return $"http://{localIP}:5091";
        }
        catch
        {
            return "http://localhost:5091";
        }
    }
}