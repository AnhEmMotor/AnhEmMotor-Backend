namespace Application.ApiContracts.ContractTemplate.Requests;

public sealed record UpdateContractTemplateRequest
{
    public string Name { get; init; } = string.Empty;
    public string Type { get; init; } = string.Empty;
    public string Code { get; init; } = string.Empty;
    public string Content { get; init; } = string.Empty;
    public string DynamicFields { get; init; } = string.Empty;
    public bool IsActive { get; init; }
    public int Status { get; init; }
}
