using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;

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

        var options = new ChromeOptions();
        options.AddArgument("--headless");
        options.AddArgument("--no-sandbox");
        options.AddArgument("--disable-dev-shm-usage");

        using var driver = new ChromeDriver(options);

        try
        {
            driver.Navigate().GoToUrl(LoginUrl);

            driver.FindElement(By.CssSelector("input[name='username'], input[type='email'], #username"))
                  .SendKeys(username);

            driver.FindElement(By.CssSelector("input[name='password'], input[type='password'], #password"))
                  .SendKeys(password);

            driver.FindElement(By.CssSelector("button[type='submit'], input[type='submit']"))
                  .Click();

            await Task.Delay(2000, ct);

            logger.LogInformation("Login completato — URL corrente: {Url}", driver.Url);
            return true;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Errore durante il login su Vecon");
            return false;
        }
        finally
        {
            driver.Quit();
        }
    }
}
