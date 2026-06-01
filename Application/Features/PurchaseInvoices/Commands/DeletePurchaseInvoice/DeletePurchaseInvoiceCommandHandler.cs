using Application.Common.Models;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.PurchaseInvoice;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Features.PurchaseInvoices.Commands.DeletePurchaseInvoice
{
    public sealed class DeletePurchaseInvoiceCommandHandler(
        IPurchaseInvoiceReadRepository readRepository,
        IPurchaseInvoiceDeleteRepository deleteRepository,
        IUnitOfWork unitOfWork) : IRequestHandler<DeletePurchaseInvoiceCommand, Result<bool>>
    {
        public async Task<Result<bool>> Handle(
            DeletePurchaseInvoiceCommand request,
            CancellationToken cancellationToken)
        {
            var invoice = await readRepository.GetByIdAsync(request.Id, cancellationToken).ConfigureAwait(false);
            if (invoice is null)
            {
                return Error.NotFound($"Không tìm thấy hóa đơn mua hàng có ID {request.Id}.", "Id");
            }

            deleteRepository.Delete(invoice);
            await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

            return true;
        }
    }
}
