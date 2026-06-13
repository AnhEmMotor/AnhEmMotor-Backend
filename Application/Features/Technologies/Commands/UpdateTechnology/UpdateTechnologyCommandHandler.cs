using Application.ApiContracts.Technology.Responses;
using Application.Common.Models;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.Technology.Technology;
using Mapster;
using MediatR;

namespace Application.Features.Technologies.Commands.UpdateTechnology
{
    public class UpdateTechnologyCommandHandler(
        ITechnologyReadRepository readRepository,
        ITechnologyUpdateRepository updateRepository,
        IUnitOfWork unitOfWork) : IRequestHandler<UpdateTechnologyCommand, Result<TechnologyResponse>>
    {
        public async Task<Result<TechnologyResponse>> Handle(
            UpdateTechnologyCommand request,
            CancellationToken cancellationToken)
        {
            var tech = await readRepository.GetByIdAsync(request.Id, cancellationToken).ConfigureAwait(false);
            if (tech == null)
            {
                return Result<TechnologyResponse>.Failure(new Error("Technology.NotFound", "Không tìm thấy công nghệ."));
            }
            tech.Name = request.Name;
            tech.CategoryId = request.CategoryId;
            tech.BrandId = request.BrandId;
            tech.DefaultTitle = request.DefaultTitle ?? request.Name;
            tech.DefaultDescription = request.DefaultDescription;
            tech.DefaultImageUrl = request.DefaultImageUrl;
            updateRepository.Update(tech);
            await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            var updatedTech = await readRepository.GetByIdAsync(tech.Id, cancellationToken).ConfigureAwait(false);
            return updatedTech!.Adapt<TechnologyResponse>();
        }
    }
}
