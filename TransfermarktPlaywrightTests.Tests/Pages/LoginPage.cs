using Microsoft.Playwright;
using static Microsoft.Playwright.Assertions;

namespace TransfermarktPlaywrightTests.Tests.Pages;

// Represents the login form at /profil/login, reached via HomePage.OpenLogin().
public class LoginPage(IPage page) : BasePage(page)
{
    public const string Url = "https://www.transfermarkt.com/profil/login";

    private ILocator UsernameInput => _page.Locator("#LoginForm_username");

    private ILocator PasswordInput => _page.Locator("#LoginForm_password");

    private ILocator SubmitButton => _page.Locator("button.login__buttons--signin");

    private ILocator ErrorMessages => _page.Locator("#login-form_es_ li");

    private ILocator RememberMeCheckbox => _page.Locator("#LoginForm_rememberMe");

    // Fills the login form and submits it. Callers assert their own expected outcome
    // (redirect + logged-in header for success, error summary for failure) since submission
    // is AJAX on failure but a full-page redirect on success.
    public async Task Login(string username, string password, bool rememberMe = false)
    {
        await UsernameInput.FillAsync(username);
        await PasswordInput.FillAsync(password);
        if (rememberMe)
        {
            await DismissCookieBannerIfShown();
            await RememberMeCheckbox.CheckAsync();
        }
        await DismissCookieBannerIfShown();
        await SubmitButton.ClickAsync();
    }

    // Waits for the validation error summary to render and returns its messages.
    public async Task<IReadOnlyList<string>> GetErrorMessages()
    {
        await Expect(ErrorMessages.First).ToBeVisibleAsync();
        return await ErrorMessages.AllTextContentsAsync();
    }

    // The site shows no visible remember-me indicator after login; the only observable signal is
    // whether the "_mcreg" cookie persists past the browser session (Expires > 0) or is
    // session-only (-1).
    public async Task<bool> HasPersistentSession()
    {
        var cookies = await _page.Context.CookiesAsync();
        var mcreg = cookies.FirstOrDefault(c => c.Name == "_mcreg");
        return mcreg is not null && mcreg.Expires > 0;
    }
}
