using Application.ApiContracts.Output.Responses;
using Application.Common.Models;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.Output;
using Domain.Constants;
using Mapster;
using MediatR;

namespace Application.Features.Outputs.Commands.UpdateOutputCompanyInvoice;

public class UpdateOutputCompanyInvoiceCommandHandler(
    IOutputReadRepository readRepository,
    IOutputUpdateRepository updateRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<UpdateOutputCompanyInvoiceCommand, Result<OrderDetailResponse>>
{
    public async Task<Result<OrderDetailResponse>> Handle(
        UpdateOutputCompanyInvoiceCommand request,
        CancellationToken cancellationToken)
    {
        var output = await readRepository.GetByIdWithDetailsAsync(
            request.Id,
            cancellationToken,
            DataFetchMode.ActiveOnly)
            .ConfigureAwait(false);

        if (output is null)
        {
            return Error.NotFound($"Không tìm thấy đơn hàng có ID {request.Id}.", "Id");
        }

        output.IsCompanyInvoice = true;
        output.CompanyName = request.CompanyName.Trim();
        output.CompanyAddress = request.CompanyAddress.Trim();
        output.CompanyTaxCode = request.CompanyTaxCode.Trim();
        output.CompanyEmail = string.IsNullOrWhiteSpace(request.CompanyEmail) ? null : request.CompanyEmail.Trim();
        output.BudgetCode = string.IsNullOrWhiteSpace(request.BudgetCode) ? null : request.BudgetCode.Trim();

        updateRepository.Update(output);
        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        var updated = await readRepository.GetByIdWithDetailsAsync(output.Id, cancellationToken).ConfigureAwait(false);
        ArgumentNullException.ThrowIfNull(updated);

        return updated.Adapt<OrderDetailResponse>();
    }
}
