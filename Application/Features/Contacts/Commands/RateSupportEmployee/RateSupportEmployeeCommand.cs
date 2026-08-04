using Application.ApiContracts.Contacts.Requests;
using Application.Common.Models;
using MediatR;

namespace Application.Features.Contacts.Commands.RateSupportEmployee;

public record RateSupportEmployeeCommand(
    int SupportRequestId,
    CustomerSupportRatingRequest Request) : IRequest<Result<bool>>;
