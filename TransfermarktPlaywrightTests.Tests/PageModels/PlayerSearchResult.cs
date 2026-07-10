namespace TransfermarktPlaywrightTests.Tests.PageModels;

// A single player hit from the quick-search results page.
public record PlayerSearchResult(
    string Name,
    string ProfileUrl);
