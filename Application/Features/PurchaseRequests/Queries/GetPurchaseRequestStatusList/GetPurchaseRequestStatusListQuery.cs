using Application.Common.Models;
using MediatR;
using System.Collections.Generic;

namespace Application.Features.PurchaseRequests.Queries.GetPurchaseRequestStatusList;

public sealed record GetPurchaseRequestStatusListQuery : IRequest<Result<Dictionary<string, string>>>;
