using Microsoft.Playwright;
using static Microsoft.Playwright.Assertions;

namespace TransfermarktPlaywrightTests.Tests.Pages;

// Represents the login overlay opened from the header's "Log in" button.
public class LoginPage(IPage page) : BasePage(page)
{
    // Username input field.
    private ILocator UsernameInput => _page.Locator("#username");

    // Password input field.
    private ILocator PasswordInput => _page.Locator("#password");

    // Submit login button.
    private ILocator SubmitButton => _page.GetByRole(AriaRole.Button, new() { Name = "Login", Exact = true });

    // The inline error shown after a failed submit (e.g. wrong credentials).
    private ILocator ErrorMessage => _page.Locator("p.form-error");

    // "Remember me" checkbox.
    private ILocator RememberMeCheckbox => _page.Locator("#LoginForm_rememberMe");

    // Fills the username/password (and optionally "remember me") without submitting.
    public async Task FillCredentials(string username, string password, bool rememberMe = false)
    {
        await UsernameInput.FillAsync(username);
        await PasswordInput.FillAsync(password);
        if (rememberMe)
        {
            await RememberMeCheckbox.CheckAsync();
        }
    }

    // Fills and submits the form, waits for the login response.
    public async Task Login(string username, string password, bool rememberMe = false)
    {
        await FillCredentials(username, password, rememberMe);

        await _page.RunAndWaitForResponseAsync(
            () => SubmitButton.ClickAsync(),
            response => response.Url.Contains("/profil/login") && response.Request.Method == "POST");
    }

    // True if the submit button is disabled (e.g. a required field is blank).
    public async Task<bool> IsSubmitDisabled()
    {
        return await SubmitButton.IsDisabledAsync();
    }

    // Waits for the inline error to render and returns its text.
    public async Task<string> GetErrorMessage()
    {
        await Expect(ErrorMessage).ToBeVisibleAsync();
        return await ErrorMessage.InnerTextAsync();
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
