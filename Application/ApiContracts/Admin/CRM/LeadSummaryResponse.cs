namespace Application.ApiContracts.Admin.CRM
{
    public record LeadSummaryResponse(
        int Id,
        string CustomerName,
        string PhoneNumber,
        string Priority,
        string Status,
        DateTime CreatedAt);

    public record CustomerTimelineDto(string EventDate, string Action, string Note, string StaffName);

    public record Customer360Response(
        string FullName,
        string Phone,
        List<string> OwnedVehicles,
        List<CustomerTimelineDto> Timeline,
        string InternalNotes);

    public record DistributeLeadRequest(string SaleId);
}
