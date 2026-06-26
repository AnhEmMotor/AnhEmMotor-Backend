using Application.Common.Models;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.SupplierDebt;
using Domain.Entities;
using MediatR;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Features.DebtPayments.Commands.UpdateDebtProofImages
{
    public class UpdateDebtProofImagesCommandHandler(
        ISupplierDebtReadRepository supplierDebtReadRepository,
        ISupplierDebtUpdateRepository supplierDebtUpdateRepository,
        IUnitOfWork unitOfWork) : IRequestHandler<UpdateDebtProofImagesCommand, Result<bool>>
    {
        public async Task<Result<bool>> Handle(UpdateDebtProofImagesCommand request, CancellationToken cancellationToken)
        {
            var debtLog = await supplierDebtReadRepository.GetDebtLogByIdAsync(request.DebtLogId, cancellationToken);
            if (debtLog == null)
            {
                return Result<bool>.Failure("Không tìm thấy lần thanh toán này.");
            }

            debtLog.ProofImages ??= new List<SupplierDebtLogImage>();

            var existingUrls = debtLog.ProofImages.Select(x => x.ImageUrl).ToList();

            var urlsToRemove = existingUrls.Except(request.ProofImageUrls).ToList();
            var urlsToAdd = request.ProofImageUrls.Except(existingUrls).ToList();

            foreach (var url in urlsToRemove)
            {
                var imageToRemove = debtLog.ProofImages.First(x => x.ImageUrl == url);
                debtLog.ProofImages.Remove(imageToRemove);
            }

            foreach (var url in urlsToAdd)
            {
                debtLog.ProofImages.Add(new SupplierDebtLogImage
                {
                    ImageUrl = url
                });
            }

            supplierDebtUpdateRepository.UpdateSupplierDebtLog(debtLog);
            await unitOfWork.SaveChangesAsync(cancellationToken);

            return Result<bool>.Success(true);
        }
    }
}
