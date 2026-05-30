using Application.Common.Models;
using Domain.Constants;
using MediatR;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Features.Quotations.Queries.GetQuotationStatusList;

public sealed class GetQuotationStatusListQueryHandler : IRequestHandler<GetQuotationStatusListQuery, Result<Dictionary<string, string>>>
{
    private static readonly Dictionary<string, string> Statuses = new()
    {
        { QuotationType.Draft, "Phiếu tạm" },
        { QuotationType.Sent, "Đã gửi" },
        { QuotationType.Approved, "Đã duyệt" },
        { QuotationType.Rejected, "Đã từ chối" }
    };

    public Task<Result<Dictionary<string, string>>> Handle(
        GetQuotationStatusListQuery request,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(Result<Dictionary<string, string>>.Success(Statuses));
    }
}
