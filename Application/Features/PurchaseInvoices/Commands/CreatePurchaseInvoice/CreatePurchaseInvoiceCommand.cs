using Application.ApiContracts.PurchaseInvoice.Requests;
using Application.ApiContracts.PurchaseInvoice.Responses;
using Application.Common.Models;
using MediatR;
using System;
using System.Collections.Generic;

namespace Application.Features.PurchaseInvoices.Commands.CreatePurchaseInvoice
{
    public sealed record CreatePurchaseInvoiceCommand : IRequest<Result<PurchaseInvoiceDetailResponse?>>
    {
        public int? PurchaseOrderId { get; init; }

        public string? InvoiceNumber { get; init; }

        public DateTimeOffset InvoiceDate { get; init; }

        public DateTimeOffset? DueDate { get; init; }

        public string? Note { get; init; }

        public List<CreatePurchaseInvoiceItemRequest> Items { get; init; } = [];
    }
}
