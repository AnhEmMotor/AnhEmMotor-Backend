using Application.Common.Models;
using Domain.Constants.Input;
using MediatR;

namespace Application.Features.Inputs.Queries.GetInputStatusList;

public sealed class GetInputStatusListQueryHandler : IRequestHandler<GetInputStatusListQuery, Result<Dictionary<string, string>>>
{
    private static readonly Dictionary<string, string> Statuses = new()
    { { InputStatus.Working, "Phiếu tạm" }, { InputStatus.Finish, "Hoàn thành" }, { InputStatus.Cancel, "Đã huỷ" }, };

    public Task<Result<Dictionary<string, string>>> Handle(
        GetInputStatusListQuery request,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(Result<Dictionary<string, string>>.Success(Statuses));
    }
}
