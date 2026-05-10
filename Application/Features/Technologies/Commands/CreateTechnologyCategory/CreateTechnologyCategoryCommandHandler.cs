using Application.ApiContracts.Technology.Responses;
using Application.Common.Models;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.Technology;
using Domain.Entities;
using Mapster;
using MediatR;

namespace Application.Features.Technologies.Commands.CreateTechnologyCategory;

public sealed class CreateTechnologyCategoryCommandHandler(
    ITechnologyCategoryUpdateRepository categoryRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<CreateTechnologyCategoryCommand, Result<TechnologyCategoryResponse>>
{
    public async Task<Result<TechnologyCategoryResponse>> Handle(
        CreateTechnologyCategoryCommand request,
        CancellationToken cancellationToken)
    {
        var category = new TechnologyCategory { Name = request.Name };
        categoryRepository.Add(category);
        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return category.Adapt<TechnologyCategoryResponse>();
    }
}
