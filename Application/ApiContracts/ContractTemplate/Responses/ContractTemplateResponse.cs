namespace Application.ApiContracts.ContractTemplate.Responses;

public sealed record ContractTemplateResponse
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Type { get; init; } = string.Empty;
    public string Code { get; init; } = string.Empty;
    public decimal Version { get; init; }
    public string Content { get; init; } = string.Empty;
    public string DynamicFields { get; init; } = string.Empty;
    public bool IsActive { get; init; }
    public int Status { get; init; }
    public Guid? ParentId { get; init; }
    public bool IsUsed { get; init; }
}
