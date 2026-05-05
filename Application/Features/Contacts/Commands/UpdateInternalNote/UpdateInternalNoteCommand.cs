using Application.Common.Models;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.Contact;
using MediatR;

namespace Application.Features.Contacts.Commands.UpdateInternalNote;

public record UpdateInternalNoteCommand : IRequest<Result<bool>>
{
    public int ContactId { get; init; }

    public string InternalNote { get; init; } = string.Empty;
}


