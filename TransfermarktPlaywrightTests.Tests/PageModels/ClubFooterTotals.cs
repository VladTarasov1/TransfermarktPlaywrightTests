namespace TransfermarktPlaywrightTests.Tests.PageModels;

public record ClubFooterTotals(
    int SquadTotal,
    double AverageAge,
    int ForeignersTotal,
    decimal AverageMarketValueEur,
    decimal TotalMarketValueEur);
