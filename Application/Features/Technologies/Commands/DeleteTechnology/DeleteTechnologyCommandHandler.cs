using Application.Common.Models;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.Technology.Technology;
using MediatR;

namespace Application.Features.Technologies.Commands.DeleteTechnology
{
    public class DeleteTechnologyCommandHandler(
        ITechnologyReadRepository readRepository,
        ITechnologyDeleteRepository deleteRepository,
        IUnitOfWork unitOfWork) : IRequestHandler<DeleteTechnologyCommand, Result>
    {
        public async Task<Result> Handle(DeleteTechnologyCommand request, CancellationToken cancellationToken)
        {
            var tech = await readRepository.GetByIdAsync(request.Id, cancellationToken).ConfigureAwait(false);
            if (tech == null)
            {
                return Result.Failure(new Error("Technology.NotFound", "Không tìm thấy công nghệ."));
            }
            deleteRepository.Remove(tech);
            await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            return Result.Success();
        }
    }
}
