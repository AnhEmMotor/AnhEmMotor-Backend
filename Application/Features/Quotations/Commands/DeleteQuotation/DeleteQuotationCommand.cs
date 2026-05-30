using Application.Common.Models;
using MediatR;
using System;

namespace Application.Features.Quotations.Commands.DeleteQuotation
{
    public sealed record DeleteQuotationCommand : IRequest<Result>
    {
        public int? Id { get; init; }
    }
}
