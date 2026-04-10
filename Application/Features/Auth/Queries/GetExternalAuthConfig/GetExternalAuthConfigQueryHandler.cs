using Application.ApiContracts.Auth.Responses;
using Application.Common.Models;
using MediatR;
using Microsoft.Extensions.Configuration;

namespace Application.Features.Auth.Queries.GetExternalAuthConfig;

public class GetExternalAuthConfigQueryHandler(IConfiguration configuration) : IRequestHandler<GetExternalAuthConfigQuery, Result<ExternalAuthConfigResponse>>
{
    public Task<Result<ExternalAuthConfigResponse>> Handle(
        GetExternalAuthConfigQuery request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        cancellationToken.ThrowIfCancellationRequested();

        var response = new ExternalAuthConfigResponse
        {
            GoogleClientId = configuration["Authentication:Google:ClientId"] ?? string.Empty,
            FacebookAppId = configuration["Authentication:Facebook:AppId"] ?? string.Empty
        };

        return Task.FromResult(Result<ExternalAuthConfigResponse>.Success(response));
    }
}