using Application.Common.Models;
using MediatR;
using System.Collections.Generic;

namespace Application.Features.Quotations.Queries.GetQuotationStatusList;

public sealed record GetQuotationStatusListQuery : IRequest<Result<Dictionary<string, string>>>;
