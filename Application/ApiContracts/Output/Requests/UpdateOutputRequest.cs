using Application.ApiContracts.Output.Responses;
using System.Text.Json.Serialization;

namespace Application.ApiContracts.Output.Requests;

public class UpdateOutputRequest
{
    public int Id { get; init; }

    public Guid? CurrentUserId { get; init; }

    public string? StatusId { get; init; }

    public string? Notes { get; init; }

    public ICollection<OutputInfoResponse> OutputInfos { get; init; } = [];
}
