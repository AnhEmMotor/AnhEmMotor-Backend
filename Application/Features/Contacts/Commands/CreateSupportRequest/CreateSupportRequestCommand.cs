using Application.ApiContracts.Contacts.Requests;
using Application.Common.Models;
using MediatR;

namespace Application.Features.Contacts.Commands.CreateSupportRequest;

public record CreateSupportRequestCommand(CreateSupportRequestRequest Request) : IRequest<Result<int>>
{
}
