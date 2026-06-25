using Application.ApiContracts.Contacts.Requests;
using Application.Common.Models;
using MediatR;

namespace Application.Features.Contacts.Commands.CreateJobApplication;

public record CreateJobApplicationCommand(CreateJobApplicationRequest Request) : IRequest<Result<int>>
{
}
