using Application.Common.Models;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.Supplier;
using MediatR;
using System.Text.RegularExpressions;
using SupplierEntity = Domain.Entities.Supplier;

namespace Application.Features.Suppliers.Commands.CloneManySuppliers;

public class CloneManySuppliersCommandHandler(
    ISupplierReadRepository readRepository,
    ISupplierInsertRepository insertRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<CloneManySuppliersCommand, Result>
{
    public async Task<Result> Handle(CloneManySuppliersCommand request, CancellationToken cancellationToken)
    {
        if (request.Ids == null || request.Ids.Count == 0)
        {
            return Result.Success();
        }
        if (request.Ids.Count > 1)
        {
            return Result.Failure(Error.BadRequest("Chỉ được phép nhân bản 1 đối tác duy nhất mỗi lần."));
        }
        foreach (var id in request.Ids)
        {
            var existingSupplier = await readRepository.GetByIdAsync(id, cancellationToken).ConfigureAwait(false);
            if (existingSupplier == null)
            {
                continue;
            }
            string baseName = existingSupplier.Name ?? "No Name";
            var match = Regex.Match(baseName, @"^(.*?)(?:\s*\(Copy(?: (\d+))?\))?$");
            string cleanName = match.Groups[1].Value;
            string newName = $"{cleanName} (Copy)";
            int counter = 1;
            while (true)
            {
                var nameExists = await readRepository.IsNameExistsAsync(newName, null, cancellationToken)
                    .ConfigureAwait(false);
                if (!nameExists)
                {
                    break;
                }
                counter++;
                newName = $"{cleanName} (Copy {counter})";
            }
            var clonedSupplier = new SupplierEntity
            {
                Name = newName,
                Phone = existingSupplier.Phone,
                Email = existingSupplier.Email,
                StatusId = existingSupplier.StatusId,
                Notes = existingSupplier.Notes,
                Address = existingSupplier.Address,
                TaxIdentificationNumber = existingSupplier.TaxIdentificationNumber,
                PartnerTypeId = existingSupplier.PartnerTypeId
            };
            insertRepository.Add(clonedSupplier);
        }
        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return Result.Success();
    }
}
