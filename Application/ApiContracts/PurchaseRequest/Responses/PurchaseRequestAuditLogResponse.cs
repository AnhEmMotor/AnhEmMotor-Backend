namespace Application.ApiContracts.PurchaseRequest.Responses
{
    public class PurchaseRequestAuditLogResponse
    {
        public int Id { get; set; }

        public int PurchaseRequestId { get; set; }

        public string Action { get; set; } = string.Empty;

        public Guid? ChangedById { get; set; }

        public string? ChangedByName { get; set; }

        public DateTimeOffset ChangedAt { get; set; }

        public string? OldStatusId { get; set; }

        public string? NewStatusId { get; set; }

        public string? OldNotes { get; set; }

        public string? NewNotes { get; set; }

        public List<PurchaseRequestItemAuditLogResponse> ItemLogs { get; set; } = new();
    }
}
