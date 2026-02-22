using Application.Common.Models;
using MediatR;

namespace Application.Features.Inputs.Queries.GetInputStatusList;

public sealed class GetInputStatusListQueryHandler : IRequestHandler<GetInputStatusListQuery, Result<Dictionary<string, string>>>
{
    private static readonly Dictionary<string, string> Statuses = new()
    {
        { Domain.Constants.Input.InputStatus.Working, "Phiếu tạm" },
        { Domain.Constants.Input.InputStatus.Finish, "Hoàn thành" },
        { Domain.Constants.Input.InputStatus.Cancel, "Đã huỷ" },
    };

    public Task<Result<Dictionary<string, string>>> Handle(
        GetInputStatusListQuery request,
        CancellationToken cancellationToken)
    {
        return Task.FromResult(Result<Dictionary<string, string>>.Success(Statuses));
    }
}
