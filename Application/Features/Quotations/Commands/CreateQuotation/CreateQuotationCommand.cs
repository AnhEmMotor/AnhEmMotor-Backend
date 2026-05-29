using Application.ApiContracts.InventoryReceipt.Requests;
using Application.ApiContracts.InventoryReceipt.Responses;
using Application.ApiContracts.Quotation.Requests;
using Application.ApiContracts.Quotation.Responses;
using Application.Common.Models;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Features.Quotations.Commands.CreateQuotation
{
    public sealed record CreateQuotationCommand : IRequest<Result<QuotationDetailResponse?>>
    {
        public int? SupplierId { get; init; }


        public string? Notes { get; init; }

        public List<CreateQuotationItemRequest> Products { get; init; } = [];
    }
}
