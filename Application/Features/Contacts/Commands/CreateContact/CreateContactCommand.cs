using Application.Common.Models;
using MediatR;

namespace Application.Features.Contacts.Commands.CreateContact;

public record CreateContactCommand : IRequest<Result<int>>
{
    public string FullName { get; init; } = string.Empty;

    public string Email { get; init; } = string.Empty;

    public string PhoneNumber { get; init; } = string.Empty;

    public string Subject { get; init; } = string.Empty;

    public string Message { get; init; } = string.Empty;
}

