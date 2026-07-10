namespace TransfermarktPlaywrightTests.Tests.Models.ApiModels;

// A single competition entry as returned by the quickselect JSON endpoint.
public record CompetitionEntry(string Id, string Name, string Link);
