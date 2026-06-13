using Application.ApiContracts.Technology.Responses;
using Application.Common.Models;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.Technology.Technology;
using Domain.Entities;
using Mapster;
using MediatR;

namespace Application.Features.Technologies.Commands.CreateTechnology;

public class CreateTechnologyCommandHandler(
    ITechnologyReadRepository readRepository,
    ITechnologyInsertRepository insertRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<CreateTechnologyCommand, Result<TechnologyResponse>>
{
    public async Task<Result<TechnologyResponse>> Handle(
        CreateTechnologyCommand request,
        CancellationToken cancellationToken)
    {
        var tech = new Technology
        {
            Name = request.Name,
            CategoryId = request.CategoryId,
            BrandId = request.BrandId,
            DefaultTitle = request.DefaultTitle ?? request.Name,
            DefaultDescription = request.DefaultDescription,
            DefaultImageUrl = request.DefaultImageUrl
        };
        insertRepository.Add(tech);
        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        var result = await readRepository.GetByIdAsync(tech.Id, cancellationToken).ConfigureAwait(false);
        return result!.Adapt<TechnologyResponse>();
    }
}
