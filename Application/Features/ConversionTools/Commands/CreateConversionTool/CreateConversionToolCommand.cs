using Application.ApiContracts.ConversionTools.Responses;
using MediatR;

namespace Application.Features.ConversionTools.Commands.CreateConversionTool;

public record CreateConversionToolCommand : IRequest<Result<ConversionToolResponse>>
{
    public string Type { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Content { get; set; }
    public int? DelaySeconds { get; set; }
    public string? Pages { get; set; }
    public bool IsActive { get; set; }
    public string? ImageUrl { get; set; }
    public string? Url { get; set; }
    public string? Status { get; set; }
}
