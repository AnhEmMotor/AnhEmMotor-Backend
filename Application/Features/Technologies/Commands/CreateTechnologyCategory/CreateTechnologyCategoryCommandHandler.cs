using Application.ApiContracts.Technology.Responses;
using Application.Common.Models;
using Application.Interfaces.Repositories.Technology;
using Domain.Entities;
using Mapster;
using MediatR;
using Application.Interfaces.Repositories;

namespace Application.Features.Technologies.Commands.CreateTechnologyCategory;

public sealed record CreateTechnologyCategoryCommand(string Name) : IRequest<Result<TechnologyCategoryResponse>>;

public sealed class CreateTechnologyCategoryCommandHandler(ITechnologyCategoryRepository categoryRepository, IUnitOfWork unitOfWork) : IRequestHandler<CreateTechnologyCategoryCommand, Result<TechnologyCategoryResponse>>
{
    public async Task<Result<TechnologyCategoryResponse>> Handle(CreateTechnologyCategoryCommand request, CancellationToken cancellationToken)
    {
        var category = new TechnologyCategory { Name = request.Name };
        categoryRepository.Add(category);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return category.Adapt<TechnologyCategoryResponse>();
    }
}
