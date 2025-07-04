using MudBlazor.Services;
using BogCraft.UI.Components;
using BogCraft.UI.Services;
using BogCraft.UI.Hubs;

var builder = WebApplication.CreateBuilder(args);

// Add MudBlazor services
builder.Services.AddMudServices();

// Add SignalR
builder.Services.AddSignalR();

// Add custom services in correct order
builder.Services.AddSingleton<ISettingsService, SettingsService>();
builder.Services.AddSingleton<ILogService, LogService>();
builder.Services.AddSingleton<IMinecraftServerService, MinecraftServerService>();
builder.Services.AddScoped<IThemeService, ThemeService>();
builder.Services.AddHostedService<AutoRestartService>();

// Add services to the container
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

var app = builder.Build();

// Configure the HTTP request pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

// Map SignalR hub
app.MapHub<ServerConsoleHub>("/serverhub");

app.Run();