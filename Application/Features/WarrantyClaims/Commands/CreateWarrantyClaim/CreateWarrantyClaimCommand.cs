using Application.Common.Models;
using MediatR;
using System.Collections.Generic;

namespace Application.Features.WarrantyClaims.Commands.CreateWarrantyClaim
{
    public class CreateWarrantyClaimPartDto
    {
        public string PartName { get; set; } = string.Empty;
        public string PartCode { get; set; } = string.Empty;
        public decimal UnitPrice { get; set; }
    }

    public class CreateWarrantyClaimCommand : IRequest<Result<int>>
    {
        public string LicensePlate { get; set; } = string.Empty;
        public string IssueDescription { get; set; } = string.Empty;
        public string? ServiceCenterName { get; set; }
        public string? ManufacturerClaimNumber { get; set; }
        public string? MediaUrls { get; set; }
        public List<CreateWarrantyClaimPartDto> Parts { get; set; } = new();
    }
}
