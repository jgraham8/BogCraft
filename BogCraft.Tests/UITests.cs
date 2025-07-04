using Microsoft.Playwright;

namespace BogCraft.Tests;

[TestClass]
public class UITests : PageTest
{
    private const string BaseUrl = "http://localhost:5000";

    [TestMethod]
    public async Task Dashboard_LoadsAndShowsServerStatus()
    {
        await Page.GotoAsync(BaseUrl);
        
        await Expect(Page.GetByText("BogCraft Server Control")).ToBeVisibleAsync();
        await Expect(Page.GetByText("Server Status")).ToBeVisibleAsync();
        await Expect(Page.Locator("text=ONLINE, text=OFFLINE").First).ToBeVisibleAsync();
    }

    [TestMethod]
    public async Task Navigation_AllPagesAccessible()
    {
        await Page.GotoAsync(BaseUrl);
        
        await Page.GetByRole(AriaRole.Link, new() { Name = "Console" }).ClickAsync();
        await Expect(Page.GetByText("Server Console")).ToBeVisibleAsync();
        
        await Page.GetByRole(AriaRole.Link, new() { Name = "Players" }).ClickAsync();
        await Expect(Page.GetByText("Players Online")).ToBeVisibleAsync();
        
        await Page.GetByRole(AriaRole.Link, new() { Name = "Log Archive" }).ClickAsync();
        await Expect(Page.GetByText("Archived Log Sessions")).ToBeVisibleAsync();
        
        await Page.GetByRole(AriaRole.Link, new() { Name = "Settings" }).ClickAsync();
        await Expect(Page.GetByText("Server Settings")).ToBeVisibleAsync();
    }

    [TestMethod]
    public async Task Console_OutputContainerHasCorrectSizing()
    {
        await Page.GotoAsync($"{BaseUrl}/console");
        
        var consoleOutput = Page.Locator(".console-output");
        await Expect(consoleOutput).ToBeVisibleAsync();
        
        var boundingBox = await consoleOutput.BoundingBoxAsync();
        Assert.IsNotNull(boundingBox);
        Assert.AreEqual(500, boundingBox.Height, 10);
        
        var overflowY = await consoleOutput.EvaluateAsync<string>("el => getComputedStyle(el).overflowY");
        Assert.AreEqual("auto", overflowY);
    }

    [TestMethod]
    public async Task Console_LongTextDoesNotOverflow()
    {
        await Page.GotoAsync($"{BaseUrl}/console");
        
        var consoleContainer = Page.Locator(".console-output");
        var containerWidth = await consoleContainer.EvaluateAsync<int>("el => el.clientWidth");
        
        var logLines = Page.Locator(".console-line");
        var count = await logLines.CountAsync();
        
        if (count > 0)
        {
            var firstLine = logLines.First;
            var lineWidth = await firstLine.EvaluateAsync<int>("el => el.scrollWidth");
            
            Assert.IsTrue(lineWidth <= containerWidth + 50, 
                "Console text should not overflow horizontally");
        }
    }

    [TestMethod]
    public async Task ServerActions_ButtonsHaveCorrectStates()
    {
        await Page.GotoAsync(BaseUrl);
        
        var startButton = Page.GetByRole(AriaRole.Button, new() { Name = "START SERVER" });
        var stopButton = Page.GetByRole(AriaRole.Button, new() { Name = "STOP SERVER" });
        
        await Expect(startButton).ToBeVisibleAsync();
        await Expect(stopButton).ToBeVisibleAsync();
        
        var startDisabled = await startButton.IsDisabledAsync();
        var stopDisabled = await stopButton.IsDisabledAsync();
        
        Assert.AreNotEqual(startDisabled, stopDisabled, 
            "Start and Stop buttons should have opposite enabled states");
    }

    [TestMethod]
    public async Task Console_FilterChipsWork()
    {
        await Page.GotoAsync($"{BaseUrl}/console");
        
        await Page.GetByText("Errors").ClickAsync();
        await Task.Delay(500);
        
        await Page.GetByText("Warnings").ClickAsync();
        await Task.Delay(500);
        
        await Page.GetByText("All").ClickAsync();
        await Task.Delay(500);
        
        await Expect(Page.Locator(".console-output")).ToBeVisibleAsync();
    }

    [TestMethod]
    public async Task Settings_FormFieldsWork()
    {
        await Page.GotoAsync($"{BaseUrl}/settings");
        
        var serverPathField = Page.GetByLabel("Server Directory");
        await Expect(serverPathField).ToBeVisibleAsync();
        
        var autoRestartToggle = Page.GetByLabel("Enable Auto-Restart");
        await Expect(autoRestartToggle).ToBeVisibleAsync();
        
        var initialState = await autoRestartToggle.IsCheckedAsync();
        await autoRestartToggle.ClickAsync();
        await Task.Delay(500);
        
        var newState = await autoRestartToggle.IsCheckedAsync();
        Assert.AreNotEqual(initialState, newState, "Auto-restart toggle should change state");
    }

    [TestMethod]
    public async Task ThemeToggle_Works()
    {
        await Page.GotoAsync(BaseUrl);
        
        var themeButton = Page.Locator("[title='Toggle Theme']");
        await Expect(themeButton).ToBeVisibleAsync();
        
        var initialTheme = await Page.EvaluateAsync<string>(
            "() => document.documentElement.getAttribute('data-theme')");
        
        await themeButton.ClickAsync();
        await Task.Delay(1000);
        
        var newTheme = await Page.EvaluateAsync<string>(
            "() => document.documentElement.getAttribute('data-theme')");
        
        Assert.AreNotEqual(initialTheme, newTheme, "Theme should change when toggle is clicked");
    }
}