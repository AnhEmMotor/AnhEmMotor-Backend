using Application.Common.Models;
using Domain.Enums;
using MediatR;

namespace Application.Features.WarrantyClaims.Commands.UpdateWarrantyClaimStatus
{
    public class UpdateWarrantyClaimStatusCommand : IRequest<Result<bool>>
    {
        public int Id { get; set; }
        public WarrantyClaimStatus Status { get; set; }
        public bool? IsRecall { get; set; }
        public string? ManufacturerDecision { get; set; }
    }
}
