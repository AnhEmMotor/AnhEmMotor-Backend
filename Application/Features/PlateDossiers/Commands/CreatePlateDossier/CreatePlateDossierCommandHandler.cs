using Application.Common.Models;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.PlateDossier;
using Domain.Entities;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Features.PlateDossiers.Commands.CreatePlateDossier
{
    public class CreatePlateDossierCommandHandler(
        IPlateDossierUpdateRepository plateDossierUpdateRepository,
        IPlateDossierReadRepository plateDossierReadRepository,
        IUnitOfWork unitOfWork) : IRequestHandler<CreatePlateDossierCommand, Result<int>>
    {
        public async Task<Result<int>> Handle(CreatePlateDossierCommand request, CancellationToken cancellationToken)
        {
            if (request.OutputId.HasValue)
            {
                var existing = await plateDossierReadRepository.GetByOutputIdAsync(request.OutputId.Value, cancellationToken)
                    .ConfigureAwait(false);
                if (existing != null)
                {
                    return Result<int>.Failure(Error.BadRequest("Hồ sơ biển số cho đơn hàng này đã tồn tại."));
                }
            }

            var todayStr = DateTimeOffset.UtcNow.ToString("yyyyMMdd");
            var randStr = Guid.NewGuid().ToString("N")[..4].ToUpper();
            var dossierNumber = $"PD-{todayStr}-{randStr}";

            var plateDossier = new PlateDossier
            {
                OutputId = request.OutputId,
                DossierNumber = dossierNumber,
                CustomerName = request.CustomerName,
                CustomerPhone = request.CustomerPhone,
                LicensePlate = request.LicensePlate,
                VinNumber = request.VinNumber,
                Status = request.Status,
                RegistrationFee = request.RegistrationFee,
                ActualCost = request.ActualCost,
                ServiceFee = request.ServiceFee,
                Notes = request.Notes,
                CompletedDate = request.Status == "Hoàn thành" ? (request.CompletedDate ?? DateTimeOffset.UtcNow) : null
            };

            plateDossier.CreatedAt = request.CreatedAt ?? DateTimeOffset.UtcNow;

            plateDossierUpdateRepository.Add(plateDossier);
            await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            return Result<int>.Success(plateDossier.Id);
        }
    }
}
