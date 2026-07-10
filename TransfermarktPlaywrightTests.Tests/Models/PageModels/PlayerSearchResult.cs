namespace TransfermarktPlaywrightTests.Tests.Models.PageModels;

// A single player hit from the quick-search results page.
public record PlayerSearchResult(
    string Name,
    string ProfileUrl);
