namespace TransfermarktPlaywrightTests.Tests.PageModels;

// The league overview table's footer row, summarizing all clubs.
public record ClubFooterTotals(
    int SquadTotal,
    double AverageAge,
    int ForeignersTotal,
    decimal AverageMarketValueEur,
    decimal TotalMarketValueEur);
