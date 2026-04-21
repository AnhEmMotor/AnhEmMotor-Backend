using Application.Common.Models;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.OptionValue;
using Domain.Entities;
using Domain.Primitives;
using MediatR;
using OptionValueEntity = Domain.Entities.OptionValue;

namespace Application.Features.OptionValues.Commands.DeleteOptionValue;

public record DeleteOptionValueCommand(int Id) : IRequest<Result>;

public class DeleteOptionValueCommandHandler(
    IOptionValueReadRepository optionValueReadRepository,
    IOptionValueDeleteRepository optionValueDeleteRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<DeleteOptionValueCommand, Result>
{
    public async Task<Result> Handle(DeleteOptionValueCommand request, CancellationToken cancellationToken)
    {
        var optionValue = await optionValueReadRepository.GetByIdAsync(request.Id, cancellationToken);

        if (optionValue == null)
        {
            return Result.Failure(new Error("OptionValue.NotFound", "Không tìm thấy thuộc tính yêu cầu."));
        }

        optionValueDeleteRepository.Delete(optionValue);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
