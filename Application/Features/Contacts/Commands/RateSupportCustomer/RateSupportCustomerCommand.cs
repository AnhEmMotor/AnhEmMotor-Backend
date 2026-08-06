using Application.ApiContracts.Contacts.Requests;
using Application.Common.Models;
using MediatR;

namespace Application.Features.Contacts.Commands.RateSupportCustomer;

public record RateSupportCustomerCommand(int SupportRequestId, SupportRatingRequest Request) : IRequest<Result<bool>>;
