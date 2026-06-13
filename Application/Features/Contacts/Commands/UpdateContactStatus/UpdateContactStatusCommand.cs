using Application.ApiContracts.Contacts.Requests;
using Application.Common.Models;
using MediatR;

namespace Application.Features.Contacts.Commands.UpdateContactStatus;

public record UpdateContactStatusCommand(string ContactType, int Id, UpdateContactStatusRequest Request) : IRequest<Result<bool>>
{
}
