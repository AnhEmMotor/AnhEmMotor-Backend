using Application.ApiContracts.User.Responses;
using Application.Common.Models;
using Domain.Constants;
using MediatR;

namespace Application.Features.Users.Queries.GetGenderOptions;

public class GetGenderOptionsQueryHandler : IRequestHandler<GetGenderOptionsQuery, Result<IEnumerable<GenderOptionResponse>>>
{
    public Task<Result<IEnumerable<GenderOptionResponse>>> Handle(
        GetGenderOptionsQuery request,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var options = new List<GenderOptionResponse>
        {
            new() { Key = GenderStatus.Male, Label = "Nam" },
            new() { Key = GenderStatus.Female, Label = "Nữ" },
            new() { Key = GenderStatus.Other, Label = "Khác" }
        };
        return Task.FromResult(Result<IEnumerable<GenderOptionResponse>>.Success(options));
    }
}
