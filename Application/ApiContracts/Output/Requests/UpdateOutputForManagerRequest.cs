using Application.ApiContracts.Output.Responses;
using System.Text.Json.Serialization;

namespace Application.ApiContracts.Output.Requests;

public class UpdateOutputForManagerRequest
{
    public int Id { get; init; }

    public string? StatusId { get; init; }

    public Guid? CurrentUserId { get; init; }

    public string? Notes { get; init; }

    public ICollection<OutputInfoResponse> OutputInfos { get; init; } = [];
}
