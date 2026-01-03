using Application.ApiContracts.Supplier.Responses;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.Supplier;
using Domain.Entities;
using Mapster;
using MediatR;

namespace Application.Features.Suppliers.Commands.CreateSupplier;

public sealed class CreateSupplierCommandHandler(ISupplierInsertRepository repository, ISupplierReadRepository supplierReadRepository, IUnitOfWork unitOfWork) : IRequestHandler<CreateSupplierCommand, SupplierResponse>
{
    public async Task<SupplierResponse> Handle(CreateSupplierCommand request, CancellationToken cancellationToken)
    {
        var supplier = request.Adapt<Supplier>();

        repository.Add(supplier);
        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return supplier.Adapt<SupplierResponse>();
    }
}
