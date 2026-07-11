using Application.ApiContracts.Customer.Responses;
using Application.Common.Models;
using MediatR;

namespace Application.Features.Customer.Queries.GetCustomerProfile360;

public sealed record GetCustomerProfile360Query(int LeadId) : IRequest<Result<CustomerProfile360Response>>;
