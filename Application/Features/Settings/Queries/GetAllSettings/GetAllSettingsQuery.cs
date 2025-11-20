using MediatR;

namespace Application.Features.Settings.Queries.GetAllSettings;

public sealed record GetAllSettingsQuery : IRequest<Dictionary<string, long?>>;
