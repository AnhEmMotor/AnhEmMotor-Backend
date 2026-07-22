using Application.Common.Models;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.WarrantyTerm;
using Domain.Entities;
using MediatR;
using Mapster;

namespace Application.Features.WarrantyTerms.Commands.CreateWarrantyTerm;

public class CreateWarrantyTermCommandHandler(
    IWarrantyTermInsertRepository insertRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<CreateWarrantyTermCommand, Result<int>>
{
    public async Task<Result<int>> Handle(CreateWarrantyTermCommand request, CancellationToken cancellationToken)
    {
        var warrantyTerm = request.Adapt<Domain.Entities.WarrantyTerm>();
        insertRepository.Add(warrantyTerm);
        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return Result<int>.Success(warrantyTerm.Id);
    }
}
