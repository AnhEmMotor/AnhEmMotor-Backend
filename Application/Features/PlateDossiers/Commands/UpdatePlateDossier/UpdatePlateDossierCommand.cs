using Application.Common.Models;
using MediatR;
using System;

namespace Application.Features.PlateDossiers.Commands.UpdatePlateDossier
{
    public class UpdatePlateDossierCommand : IRequest<Result<bool>>
    {
        public int Id { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public string CustomerPhone { get; set; } = string.Empty;
        public string LicensePlate { get; set; } = string.Empty;
        public string VinNumber { get; set; } = string.Empty;
        public string Status { get; set; } = "Prepare";
        public decimal RegistrationFee { get; set; }
        public decimal ActualCost { get; set; }
        public decimal ServiceFee { get; set; }
        public string? Notes { get; set; }
        public DateTimeOffset? CompletedDate { get; set; }
    }
}
