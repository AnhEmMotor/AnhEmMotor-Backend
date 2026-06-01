using Application.Common.Models;
using MediatR;
using System.Collections.Generic;

namespace Application.Features.PurchaseOrders.Queries.GetPurchaseOrderStatusList
{
    public sealed record GetPurchaseOrderStatusListQuery : IRequest<Result<Dictionary<string, string>>>;
}
