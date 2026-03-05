using Application.ApiContracts.Input.Responses;
using Application.Common.Models;
using MediatR;
using System.Text.Json.Serialization;

namespace Application.Features.Inputs.Commands.UpdateInputNotes;

public sealed record UpdateInputNotesCommand : IRequest<Result<InputDetailResponse>>
{
    public int Id { get; init; }

    public string? Notes { get; init; }
}
