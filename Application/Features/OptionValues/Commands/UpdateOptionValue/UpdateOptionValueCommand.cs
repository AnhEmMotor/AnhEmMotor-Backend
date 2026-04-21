using Application.Common.Models;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.OptionValue;
using Domain.Entities;
using Domain.Primitives;
using MediatR;
using OptionValueEntity = Domain.Entities.OptionValue;

namespace Application.Features.OptionValues.Commands.UpdateOptionValue;

public record UpdateOptionValueCommand : IRequest<Result>
{
    public int Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string? ColorCode { get; init; }
}

public class UpdateOptionValueCommandHandler(
    IOptionValueReadRepository optionValueReadRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<UpdateOptionValueCommand, Result>
{
    public async Task<Result> Handle(UpdateOptionValueCommand request, CancellationToken cancellationToken)
    {
        var optionValue = await optionValueReadRepository.GetByIdAsync(request.Id, cancellationToken);

        if (optionValue == null)
        {
            return Result.Failure(new Error("OptionValue.NotFound", "Không tìm thấy thuộc tính yêu cầu."));
        }

        optionValue.Name = request.Name;
        optionValue.ColorCode = request.ColorCode;

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
