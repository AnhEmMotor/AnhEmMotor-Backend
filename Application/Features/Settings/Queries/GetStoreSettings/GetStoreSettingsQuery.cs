using Application.Common.Models;
using MediatR;

namespace Application.Features.Settings.Queries.GetStoreSettings;

public class GetStoreSettingsQuery : IRequest<Result<Dictionary<string, string?>>>
{
}
