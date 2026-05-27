using Application.ApiContracts.Input.Requests;
using Application.ApiContracts.Quotation.Requests;
using Application.ApiContracts.Quotation.Responses;
using Application.Common.Models;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Features.Quotations.Commands.UpdateQuotation
{
    public sealed record UpdateQuotationCommand : IRequest<Result<QuotationDetailResponse?>>
    {
        public int? Id { get; init; }

        public int? SupplierId { get; init; }

        public string? Status { get; init; }

        public int? Notes { get; init; }

        public List<UpdateQuotationItemRequest> Products { get; init; } = [];
    }
}
