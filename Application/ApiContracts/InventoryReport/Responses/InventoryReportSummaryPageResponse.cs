namespace Application.ApiContracts.InventoryReport.Responses
{
    public sealed record InventoryReportSummaryPageResponse(
        List<InventoryReportSummaryResponse> Items,
        int TotalCount,
        int PageNumber,
        int PageSize);
}
