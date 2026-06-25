using MediatR;
using System;

namespace Application.Features.Logistics.Returns.Commands.InspectReturn;

public class InspectReturnCommand : IRequest<bool>
{
    public int Id { get; set; }

    public string? BoxCondition { get; set; }

    public string? ProductCondition { get; set; }

    public string? ReturnProofImage { get; set; }

    public string? ReturnInternalNote { get; set; }

    public string Action { get; set; } = string.Empty;
}

