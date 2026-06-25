using Application.ApiContracts.Service.Responses;
using Application.Common.Models;
using MediatR;

namespace Application.Features.Services.Commands;

public class UpdateServiceCommand : IRequest<Result<ServiceResponse>>
{
    public int Id { get; set; }

    public required string Name { get; set; }

    public string? Description { get; set; }

    public decimal BasePrice { get; set; }

    public int? EstimatedDurationMinutes { get; set; }

    public int CategoryId { get; set; }
}
