using Application.Common.Models;
using MediatR;

namespace Application.Features.PlateDossiers.Commands.UpdatePlateDossierStatus
{
    public class UpdatePlateDossierStatusCommand : IRequest<Result<bool>>
    {
        public int Id { get; set; }

        public string Status { get; set; } = string.Empty;

        public string? LicensePlate { get; set; }

        public decimal? RegistrationFee { get; set; }

        public decimal? ActualCost { get; set; }

        public decimal? ServiceFee { get; set; }

        public string? Notes { get; set; }
    }
}
