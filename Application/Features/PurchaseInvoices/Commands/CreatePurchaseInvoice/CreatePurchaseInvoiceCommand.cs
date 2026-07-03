using Application.ApiContracts.PurchaseInvoice.Requests;
using Application.ApiContracts.PurchaseInvoice.Responses;
using Application.Common.Models;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.ProductVariant;
using Application.Interfaces.Repositories.PurchaseInvoice;
using Application.Interfaces.Repositories.Supplier;
using Application.Interfaces.Services;
using Domain.Constants;
using Domain.Entities;
using Mapster;
using MediatR;
using System;
using System.Linq;

namespace Application.Features.PurchaseInvoices.Commands.CreatePurchaseInvoice
{
    public sealed record CreatePurchaseInvoiceCommand : IRequest<Result<PurchaseInvoiceDetailResponse?>>
    {
        public int? PurchaseRequestId { get; init; }
        public string? InvoiceNumber { get; init; }
        public DateTimeOffset InvoiceDate { get; init; }
        public DateTimeOffset? DueDate { get; init; }
        public int? SupplierId { get; init; }
        public string? SupplierName { get; init; }
        public string? SupplierPhone { get; init; }
        public string? SupplierAddress { get; init; }
        public string? SupplierTaxCode { get; init; }
        public string? CustomerName { get; init; }
        public string? CustomerPhone { get; init; }
        public string? CustomerAddress { get; init; }
        public string? CustomerIdCard { get; init; }
        public string? PaymentMethod { get; init; }
        public string? Notes { get; init; }
        public List<CreatePurchaseInvoiceItemRequest> Items { get; init; } = new();
    }
}
