using Domain.Constants.Order;
using FluentValidation;

namespace Application.Features.Outputs.Commands.UpdateManyOutputStatus;

public sealed class UpdateManyOutputStatusCommandValidator : AbstractValidator<UpdateManyOutputStatusCommand>
{
    public UpdateManyOutputStatusCommandValidator()
    {
        RuleFor(x => x.Ids)
            .NotEmpty().WithMessage("Danh sách ID không được để trống.")
            .Must(ids => ids.Count <= 100).WithMessage("Chỉ được cập nhật tối đa 100 đơn hàng một lần.");

        RuleFor(x => x.StatusId)
            .NotEmpty().WithMessage("Trạng thái không được để trống.")
            .Must(OrderStatus.IsValid).WithMessage(x => $"Trạng thái '{x.StatusId}' không hợp lệ trong hệ thống.");
    }
}