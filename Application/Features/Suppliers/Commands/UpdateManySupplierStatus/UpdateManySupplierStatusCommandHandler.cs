using Application.ApiContracts.Supplier;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.Supplier;
using Domain.Constants;
using Domain.Helpers;
using MediatR;

namespace Application.Features.Suppliers.Commands.UpdateManySupplierStatus;

public sealed class UpdateManySupplierStatusCommandHandler(ISupplierSelectRepository selectRepository, ISupplierUpdateRepository updateRepository, IUnitOfWork unitOfWork)
    : IRequestHandler<UpdateManySupplierStatusCommand, (List<SupplierResponse>? Data, ErrorResponse? Error)>
{
    public async Task<(List<SupplierResponse>? Data, ErrorResponse? Error)> Handle(UpdateManySupplierStatusCommand request, CancellationToken cancellationToken)
    {
        if (request.Updates == null || request.Updates.Count == 0)
        {
            return ([], null);
        }

        var errorDetails = new List<ErrorDetail>();
        var ids = request.Updates.Keys.ToList();

        foreach (var (id, status) in request.Updates)
        {
            if (!SupplierStatus.IsValid(status))
            {
                errorDetails.Add(new ErrorDetail
                {
                    Field = $"Updates[{id}]",
                    Message = $"Invalid status '{status}'. Must be one of: {string.Join(", ", SupplierStatus.AllowedValues)}"
                });
            }
        }

        if (errorDetails.Count > 0)
        {
            return (null, new ErrorResponse { Errors = errorDetails });
        }

        var suppliers = await selectRepository.GetActiveSuppliersByIdsAsync(ids, cancellationToken).ConfigureAwait(false);
        var supplierMap = suppliers.ToDictionary(s => s.Id);

        foreach (var id in ids)
        {
            if (!supplierMap.ContainsKey(id))
            {
                errorDetails.Add(new ErrorDetail
                {
                    Field = "Id",
                    Message = $"Supplier with Id {id} not found."
                });
            }
        }

        if (errorDetails.Count > 0)
        {
            return (null, new ErrorResponse { Errors = errorDetails });
        }

        foreach (var (id, status) in request.Updates)
        {
            if (supplierMap.TryGetValue(id, out var supplier))
            {
                supplier.StatusId = status;
                updateRepository.Update(supplier);
            }
        }

        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        var responses = suppliers.Select(s => new SupplierResponse
        {
            Id = s.Id,
            Name = s.Name,
            Address = s.Address,
            Phone = s.Phone,
            Email = s.Email,
            StatusId = s.StatusId
        }).ToList();

        return (responses, null);
    }
}
