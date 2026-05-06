using Application.ApiContracts.Technology.Responses;
using Application.Common.Models;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.Technology;
using Domain.Entities;
using Mapster;
using MediatR;

namespace Application.Features.Technologies.Commands.CreateTechnology;

public sealed record CreateTechnologyCommand(
    string Name,
    int? CategoryId,
    int? BrandId,
    string? DefaultTitle,
    string? DefaultDescription,
    string? DefaultImageUrl) : IRequest<Result<TechnologyResponse>>;

public sealed class CreateTechnologyCommandHandler(ITechnologyRepository technologyRepository, IUnitOfWork unitOfWork) : IRequestHandler<CreateTechnologyCommand, Result<TechnologyResponse>>
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
        technologyRepository.Add(tech);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        var result = await technologyRepository.GetByIdAsync(tech.Id, cancellationToken);
        return result!.Adapt<TechnologyResponse>();
    }
}
