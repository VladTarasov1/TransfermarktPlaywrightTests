using Microsoft.Playwright;
using static Microsoft.Playwright.Assertions;

namespace TransfermarktPlaywrightTests.Tests.Pages;

// Represents the login form.
public class LoginPage(IPage page) : BasePage(page)
{
    // Page URL.
    public const string Url = "https://www.transfermarkt.com/profil/login";

    // Username input field.
    private ILocator UsernameInput => _page.Locator("#LoginForm_username");

    // Password input field.
    private ILocator PasswordInput => _page.Locator("#LoginForm_password");

    // Submit login button.
    private ILocator SubmitButton => _page.Locator("button.login__buttons--signin");

    // Validation error messages shown after a failed submit.
    private ILocator ErrorMessages => _page.Locator("#login-form_es_ li");

    // "Remember me" checkbox.
    private ILocator RememberMeCheckbox => _page.Locator("#LoginForm_rememberMe");

    // Fills and submits the form
    public async Task Login(string username, string password, bool rememberMe = false)
    {
        await UsernameInput.FillAsync(username);
        await PasswordInput.FillAsync(password);
        if (rememberMe)
        {
            await RememberMeCheckbox.CheckAsync();
        }
        await SubmitButton.ClickAsync();
    }

    // Waits for the validation errors to render and returns its messages.
    public async Task<IReadOnlyList<string>> GetErrorMessages()
    {
        await Expect(ErrorMessages.First).ToBeVisibleAsync();
        return await ErrorMessages.AllTextContentsAsync();
    }

    // True if "remember me" was applied, inferred from the "_mcreg" cookie's expiry.
    public async Task<bool> HasPersistentSession()
    {
        var cookies = await _page.Context.CookiesAsync();
        // "_mcreg" is the site's own session cookie; its expiry reflects whether "remember me" was checked.
        var mcreg = cookies.FirstOrDefault(cookie => cookie.Name == "_mcreg");
        return mcreg is not null && mcreg.Expires > 0;
    }
}
