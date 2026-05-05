using Application.Common.Models;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.OptionValue;
using MediatR;

namespace Application.Features.OptionValues.Commands.DeleteOptionValue;

public class DeleteOptionValueCommandHandler(
    IOptionValueReadRepository optionValueReadRepository,
    IOptionValueDeleteRepository optionValueDeleteRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<DeleteOptionValueCommand, Result>
{
    public async Task<Result> Handle(DeleteOptionValueCommand request, CancellationToken cancellationToken)
    {
        var optionValue = await optionValueReadRepository.GetByIdAsync(request.Id, cancellationToken).ConfigureAwait(false)         ;
        if (optionValue == null)
        {
            return Result.Failure(new Error("OptionValue.NotFound", "Không tìm thấy thuộc tính yêu cầu."));
        }
        optionValueDeleteRepository.Delete(optionValue);
        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return Result.Success();
    }
}
