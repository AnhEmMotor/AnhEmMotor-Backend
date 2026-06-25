namespace Application.ApiContracts.NewsCategory.Responses;

public sealed record NewsCategoryResponse
{
    public int Id { get; init; }

    public string Name { get; init; } = string.Empty;

    public string Slug { get; init; } = string.Empty;

    public bool IsActive { get; init; }
}
