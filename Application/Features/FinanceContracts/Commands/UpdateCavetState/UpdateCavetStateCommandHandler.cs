using Application.ApiContracts.FinanceContract.Requests;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.FinanceContract;
using Domain.Entities;
using MediatR;


namespace Application.Features.FinanceContracts.Commands.UpdateCavetState;

public sealed class UpdateCavetStateCommandHandler(
    IUnitOfWork unitOfWork,
    IFinanceContractReadRepository repository
) : IRequestHandler<UpdateCavetStateCommand>
{
    public async Task Handle(
        UpdateCavetStateCommand request,
        CancellationToken cancellationToken)
    {
        var entity = await repository.GetByIdAsync(
            request.FinanceContractId,
            cancellationToken).ConfigureAwait(false);

        if (entity is null)
        {
            throw new KeyNotFoundException(
                $"Không tìm thấy hợp đồng tài chính với Id = {request.FinanceContractId}");
        }

        entity.CavetLocation = request.Request.State switch
        {
            "FinancialCompanyHolds" => "Bank",
            "StoreHoldsOnBehalf" => "Store",
            "DeliveredToCustomer" => "Customer",
            _ => entity.CavetLocation
        };

        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }
}
