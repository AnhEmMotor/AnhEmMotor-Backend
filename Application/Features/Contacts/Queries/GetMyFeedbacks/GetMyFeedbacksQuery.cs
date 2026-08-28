using Application.ApiContracts.Contacts.Responses;
using Application.Common.Models;
using MediatR;

namespace Application.Features.Contacts.Queries.GetMyFeedbacks;

public record GetMyFeedbacksQuery(Guid CurrentUserId) : IRequest<Result<List<CustomerFeedbackResponse>>>
{
}
