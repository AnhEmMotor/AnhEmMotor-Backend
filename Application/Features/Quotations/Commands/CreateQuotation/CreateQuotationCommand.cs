using Application.ApiContracts.Input.Requests;
using Application.ApiContracts.Input.Responses;
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

        public string? Status { get; init; }

        public int? Notes { get; init; }

        public List<CreateQuotationItemRequest> Products { get; init; } = [];
    }
}
