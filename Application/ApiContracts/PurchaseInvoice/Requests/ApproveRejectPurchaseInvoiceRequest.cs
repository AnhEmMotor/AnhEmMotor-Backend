namespace Application.ApiContracts.PurchaseInvoice.Requests
{
    public class ApproveRejectPurchaseInvoiceRequest
    {
        public bool IsApproved { get; set; }

        public string? Note { get; set; }
    }
}
