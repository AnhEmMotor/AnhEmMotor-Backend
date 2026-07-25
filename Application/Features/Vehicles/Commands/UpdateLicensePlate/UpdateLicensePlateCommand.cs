using Application.Common.Models;
using MediatR;

namespace Application.Features.Vehicles.Commands.UpdateLicensePlate
{
    public class UpdateLicensePlateCommand : IRequest<Result<bool>>
    {
        public int Id { get; set; }
        public string LicensePlate { get; set; } = string.Empty;
    }
}
