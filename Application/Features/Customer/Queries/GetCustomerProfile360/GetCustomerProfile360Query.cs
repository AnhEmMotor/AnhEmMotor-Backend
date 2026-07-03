using MediatR;

namespace Application.Features.Customer.Queries.GetCustomerProfile360;

public sealed record GetCustomerProfile360Query(int LeadId) : IRequest<Application.Common.Models.Result<Application.ApiContracts.Customer.Responses.CustomerProfile360Response>>;
