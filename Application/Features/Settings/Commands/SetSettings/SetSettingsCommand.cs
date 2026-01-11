
using Application.Common.Models;
using MediatR;

namespace Application.Features.Settings.Commands.SetSettings;

public sealed record SetSettingsCommand(Dictionary<string,string?> Settings) : IRequest<Result<Dictionary<string, string?>?>>;
