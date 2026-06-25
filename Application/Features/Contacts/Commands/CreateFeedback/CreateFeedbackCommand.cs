using Application.ApiContracts.Contacts.Requests;
using Application.Common.Models;
using MediatR;

namespace Application.Features.Contacts.Commands.CreateFeedback;

public record CreateFeedbackCommand(CreateFeedbackRequest Request) : IRequest<Result<int>>
{
}
