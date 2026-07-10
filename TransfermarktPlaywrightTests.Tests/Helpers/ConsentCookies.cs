using Microsoft.Playwright;

namespace TransfermarktPlaywrightTests.Tests.Helpers;

// Pre-answers the cookie-consent banner via cookies, instead of clicking through a flaky iframe.
public static class ConsentCookies
{
    // Cookies only apply to this site.
    private const string CookieDomain = "www.transfermarkt.com";

    // Consent string recording which ads were agreed to.
    private const string EuConsentCookieValue =
        "CQnIhwAQnIhwAAGABCENCmFsAP_gAEPgAB5YLdtR_C7dCCFAADZzaLsgeIQQ1lADJsABAAQAACAFAAIQgIwCkUEAFAAAgAAAERAAIgAAAAAAAAAAAAAAAIAEKACEAAAUwAAAIAAAABAAQAAAAAAAAAAAAAAAAgABAAAAgAAEAAIAQAAAAQACAAAgAAAAAAAAAAAAAAAAAAAAAAAAAEAAAAAAkAAAAAAAABAIAAAAAAAAACAAAAAAAAAAAAAAAAAAAAAAACAAAAAAAAAAAiAAAAAAAACC3cAUBKwEKwI3gStAt2AWEgMgAVAAuABwADwAIIAZABoAEwAKoAbwA_ACEgEMARIAjgBNADAAHsAPuAjQCOAEiAPaAvMBkgEBAIXAWFCAAwAHABFAIOOgOgALAAqABwAEEAMgA0ACYAFUALoAYgA3gB-gEMARIAjgBNACjAGAAPYAfYBFgCOAEiALEAXkA9oCZAF5gMkAgIBbscADgAcAB4AF4CDgIQQgEgALACqAGIAN4AfgBgAEcAJEBAQgACAAeAWIlAOAAWABwAJgAVQAxQCGAIkARwAowBgAEcAXmAyQCAgErSQAIAC4BBykBgABYAFQAOAAgABoAEwAKoAYgA_QCGAIkARwAowBgAD7AIsARwAkQBeQD2gLzAZIA2UCAgFhSgAUAC4AMgCDgFiAO2WgCgDAAI4BAQCwoFuw.ILdtR_C7dCCFAADZzaLsgeIQQ1lADJsABAAQAACAFAAIQgIwCkUEAFAAAgAAAERAAIgAAAAAAAAAAAAAAAIAEKACEAAAUwAAAIAAAABAAQAAAAAAAAAAAAAAAAgABAAAAgAAEAAIAQAAAAQACAAAgAAAAAAAAAAAAAAAAAAAAAAAAAEAAAAAAkAAAAAAAABAIAAAAAAAAACAAAAAAAAAAAAAAAAAAAAAAACAAAAAAAAAAQiAABAAAAAC.YAAAAAAAAAAA";

    // Record ID marking this visitor as already having made a consent choice.
    private const string ConsentUuidCookieValue = "c24608b8-bfa1-47d9-867c-4722c456a4e0_58";

    // Call once per test, before the first navigation (e.g. top of [SetUp]).
    public static Task Seed(IBrowserContext context)
    {
        var expires = DateTimeOffset.UtcNow.AddYears(1).ToUnixTimeSeconds();

        return context.AddCookiesAsync(
        [
            new Cookie { Name = "euconsent-v2", Value = EuConsentCookieValue, Domain = CookieDomain, Path = "/", Expires = expires },
            new Cookie { Name = "consentUUID", Value = ConsentUuidCookieValue, Domain = CookieDomain, Path = "/", Expires = expires },
        ]);
    }
}
