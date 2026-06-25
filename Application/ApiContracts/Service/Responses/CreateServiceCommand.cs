using Application.Common.Models;
using MediatR;

namespace Application.ApiContracts.Service.Responses;

public class CreateServiceCommand : IRequest<Result<ServiceResponse>>
{
    public required string Name { get; set; }

    public string? Description { get; set; }

    public decimal BasePrice { get; set; }

    public int? EstimatedDurationMinutes { get; set; }

    public int CategoryId { get; set; }
}
