using System;

namespace Application.ApiContracts.Client.Invoices
{
    public record InvoiceSummaryResponse(int Id, string InvoiceNumber, DateTime Date, decimal TotalAmount, string Type);

    public record InvoiceDetailResponse(
        int Id,
        string InvoiceNumber,
        DateTime Date,
        decimal TotalAmount,
        List<InvoiceItemDto> Items,
        string PdfDownloadUrl);

    public record InvoiceItemDto(string ItemName, int Quantity, decimal UnitPrice, decimal Total);
}
