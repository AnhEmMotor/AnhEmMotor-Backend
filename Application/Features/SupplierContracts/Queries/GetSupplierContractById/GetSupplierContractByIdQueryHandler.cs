using System;
using Application.ApiContracts.SupplierContracts.Responses;
using Application.Common.Models;
using Application.Interfaces.Repositories.SupplierContract;
using Domain.Constants;
using Mapster;
using MediatR;

namespace Application.Features.SupplierContracts.Queries.GetSupplierContractById;

public class GetSupplierContractByIdQueryHandler(ISupplierContractReadRepository repository) : IRequestHandler<GetSupplierContractByIdQuery, Result<SupplierContractDetailResponse>>
{
    public async Task<Result<SupplierContractDetailResponse>> Handle(
        GetSupplierContractByIdQuery request,
        CancellationToken cancellationToken)
    {
        try
        {
            var entity = await repository.GetByIdAsync(request.Id, cancellationToken, DataFetchMode.All).ConfigureAwait(false);
            if (entity == null)
            {
                return Result<SupplierContractDetailResponse>.Failure("Supplier contract not found.");
            }
            return entity.Adapt<SupplierContractDetailResponse>();
        }
        catch (Exception ex)
        {
            System.IO.File.WriteAllText("D:\\backend_error.txt", ex.ToString());
            throw;
        }
    }
}
