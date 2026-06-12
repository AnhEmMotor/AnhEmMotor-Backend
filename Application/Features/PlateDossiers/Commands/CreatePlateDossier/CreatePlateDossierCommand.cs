using Application.Common.Models;
using MediatR;

namespace Application.Features.PlateDossiers.Commands.CreatePlateDossier
{
    public class CreatePlateDossierCommand : IRequest<Result<int>>
    {
        public int OutputId { get; set; }
        public decimal RegistrationFee { get; set; }
        public decimal ActualCost { get; set; }
        public decimal ServiceFee { get; set; }
        public string? Notes { get; set; }
        public string? LicensePlate { get; set; }
    }
}
