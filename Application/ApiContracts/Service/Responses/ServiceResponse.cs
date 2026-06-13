namespace Application.ApiContracts.Service.Responses;

public record ServiceResponse
{
    public int Id { get; init; }

    public string Name { get; init; } = string.Empty;

    public string? Description { get; init; }

    public decimal BasePrice { get; init; }

    public int? EstimatedDurationMinutes { get; init; }

    public int CategoryId { get; init; }

    public bool IsActive { get; init; }
}