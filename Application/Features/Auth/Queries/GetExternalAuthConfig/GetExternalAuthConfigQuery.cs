using Application.ApiContracts.Auth.Responses;
using Application.Common.Models;
using MediatR;

namespace Application.Features.Auth.Queries.GetExternalAuthConfig;

public class GetExternalAuthConfigQuery : IRequest<Result<ExternalAuthConfigResponse>>;
