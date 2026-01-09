
using MediatR;

namespace Application.Features.Settings.Commands.SetSettings;

public sealed record SetSettingsCommand(Dictionary<string,string?> Settings) : IRequest<(Dictionary<string, string?>? Data, Common.Models.ErrorResponse? Error)>;
