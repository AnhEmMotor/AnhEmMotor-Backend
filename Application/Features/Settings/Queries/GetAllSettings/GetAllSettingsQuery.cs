using Application.Common.Models;
using MediatR;

namespace Application.Features.Settings.Queries.GetAllSettings;

public sealed record GetAllSettingsQuery : IRequest<Result<Dictionary<string, string?>>>;
