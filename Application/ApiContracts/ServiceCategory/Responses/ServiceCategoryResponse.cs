using Domain.Entities;

namespace Application.ApiContracts.ServiceCategory.Responses;

public class ServiceCategoryResponse
{
    public int Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string? Slug { get; init; }
    public string? Description { get; init; }
    public bool IsActive { get; init; }
    public int? ParentId { get; init; }
    public int ServiceCount { get; init; }
}
