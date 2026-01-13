
using Application.Common.Models;
using MediatR;

namespace Application.Features.Settings.Commands.SetSettings;

public sealed record SetSettingsCommand() : IRequest<Result<Dictionary<string, string?>?>>
{
    public Dictionary<string, string?>? Settings { get; init; }
}
