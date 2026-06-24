using System;
using System.Collections.Generic;

namespace Application.ApiContracts.Client.Invoices
{
    public record InvoiceSummaryResponse(
        int Id, 
        string InvoiceNumber, 
        DateTime Date, 
        decimal TotalAmount, 
        string Type); // Vehicle Purchase / Service & Parts

    public record InvoiceDetailResponse(
        int Id, 
        string InvoiceNumber, 
        DateTime Date, 
        decimal TotalAmount, 
        List<InvoiceItemDto> Items, 
        string PdfDownloadUrl);

    public record InvoiceItemDto(
        string ItemName, 
        int Quantity, 
        decimal UnitPrice, 
        decimal Total);
}
