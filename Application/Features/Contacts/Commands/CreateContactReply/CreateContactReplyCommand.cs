using Application.Common.Models;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.Contact;
using Application.Interfaces.Services;
using Domain.Entities;
using MediatR;

namespace Application.Features.Contacts.Commands.CreateContactReply;

public record CreateContactReplyCommand : IRequest<Result<int>>
{
    public int ContactId { get; init; }

    public string Message { get; init; } = string.Empty;

    public bool MarkAsProcessed { get; init; } = true;
}


