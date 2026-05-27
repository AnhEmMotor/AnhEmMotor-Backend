using Application.Common.Models;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Features.Quotations.Commands.DeleteQuotation
{
    public sealed record DeleteQuotationCommand : IRequest<Result>
    {
        public int? Id { get; init; }
        public bool HasApprovePermission { get; init; }
    }
}
