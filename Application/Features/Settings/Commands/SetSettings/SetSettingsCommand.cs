using Domain.Helpers;
using MediatR;

namespace Application.Features.Settings.Commands.SetSettings;

public sealed record SetSettingsCommand(Dictionary<string, long?> Settings) : IRequest<(Dictionary<string, long?>? Data, ErrorResponse? Error)>;
