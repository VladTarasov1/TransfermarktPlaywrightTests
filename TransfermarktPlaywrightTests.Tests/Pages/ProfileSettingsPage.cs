using Microsoft.Playwright;

namespace TransfermarktPlaywrightTests.Tests.Pages;

// Represents the logged-in user's profile settings page (/profil/index), reached via
// BasePage.OpenProfileSettings(). Its page title embeds the logged-in username.
public class ProfileSettingsPage(IPage page) : BasePage(page);
