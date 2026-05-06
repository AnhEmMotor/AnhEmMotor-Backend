using Application.Common.Models;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.OptionValue;
using MediatR;

namespace Application.Features.OptionValues.Commands.UpdateOptionValue;

public class UpdateOptionValueCommandHandler(
    IOptionValueReadRepository optionValueReadRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<UpdateOptionValueCommand, Result>
{
    public async Task<Result> Handle(UpdateOptionValueCommand request, CancellationToken cancellationToken)
    {
        var optionValue = await optionValueReadRepository.GetByIdAsync(request.Id, cancellationToken)
            .ConfigureAwait(false);
        if (optionValue == null)
        {
            return Result.Failure(new Error("OptionValue.NotFound", "Không tìm thấy thuộc tính yêu cầu."));
        }
        optionValue.Name = request.Name;
        optionValue.ColorCode = request.ColorCode;
        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return Result.Success();
    }
}
