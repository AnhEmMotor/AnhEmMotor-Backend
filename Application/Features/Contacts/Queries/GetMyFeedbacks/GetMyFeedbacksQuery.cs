using Application.ApiContracts.Contacts.Responses;
using Application.Common.Models;
using Domain.Primitives;
using MediatR;
using System.Collections.Generic;

namespace Application.Features.Contacts.Queries.GetMyFeedbacks;

public record GetMyFeedbacksQuery(string PhoneNumber, string CustomerName, string Email) : IRequest<Result<List<CustomerFeedbackResponse>>>
{
}
