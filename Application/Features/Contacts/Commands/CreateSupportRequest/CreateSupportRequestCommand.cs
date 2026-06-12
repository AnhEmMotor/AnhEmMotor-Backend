using Application.ApiContracts.Contacts.Requests;
using Application.Common.Models;
using Application.Interfaces.Repositories.Contact;
using Domain.Entities;
using Domain.Enums;
using MediatR;

namespace Application.Features.Contacts.Commands.CreateSupportRequest;

public record CreateSupportRequestCommand(CreateSupportRequestRequest Request) : IRequest<Result<int>>
{
}
