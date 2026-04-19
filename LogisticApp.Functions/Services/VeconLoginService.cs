using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Playwright;

namespace LogisticApp.Functions.Services;

public class VeconLoginService(IConfiguration config, ILogger<VeconLoginService> logger) : IVeconLoginService
{
    private const string LoginUrl = "https://webapp.vecon.it/login";

    public async Task<bool> LoginAsync(CancellationToken ct = default)
    {
        var username = config["Vecon:Username"]
            ?? throw new InvalidOperationException("Vecon:Username non configurato");
        var password = config["Vecon:Password"]
            ?? throw new InvalidOperationException("Vecon:Password non configurato");

        logger.LogInformation("Avvio login su {Url}", LoginUrl);

        using var playwright = await Playwright.CreateAsync();
        await using var browser = await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
        {
            Headless = true,
        });

        var page = await browser.NewPageAsync();

        try
        {
            await page.GotoAsync(LoginUrl, new PageGotoOptions { Timeout = 30_000 });

            await page.FillAsync("input[name='username'], input[type='email'], #username", username);
            await page.FillAsync("input[name='password'], input[type='password'], #password", password);
            await page.ClickAsync("button[type='submit'], input[type='submit'], button:has-text('Login'), button:has-text('Accedi')");

            // Attendi navigazione dopo il login
            await page.WaitForLoadStateAsync(LoadState.NetworkIdle, new PageWaitForLoadStateOptions { Timeout = 15_000 });

            logger.LogInformation("Login completato — URL corrente: {Url}", page.Url);
            return true;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Errore durante il login su Vecon");
            return false;
        }
        finally
        {
            await browser.CloseAsync();
        }
    }
}
