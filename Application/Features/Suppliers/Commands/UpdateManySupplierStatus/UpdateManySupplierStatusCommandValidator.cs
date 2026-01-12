using FluentValidation;
using MediatR;
using SupplierStatusConstants = Domain.Constants.SupplierStatus;

namespace Application.Features.Suppliers.Commands.UpdateManySupplierStatus;

public sealed class UpdateManySupplierStatusCommandValidator : AbstractValidator<UpdateManySupplierStatusCommand>
{
    public UpdateManySupplierStatusCommandValidator()
    {
        RuleFor(x => x.Ids)
            .NotEmpty().WithMessage("Bạn chưa truyền danh sách Supplier ID để xoá.")
            .Must(ids => ids.Count <= 20).WithMessage("Để đảm bảo an toàn dữ liệu, chỉ được xoá tối đa 20 nhà cung cấp một lần.");

        RuleFor(x => x.StatusId)
            .NotEmpty().WithMessage("Trạng thái mới không được để trống.")
            .Must(statusId => SupplierStatusConstants.IsValid(statusId)) // Input ở đây là string statusId
            .WithMessage((command, statusId) => $"Trạng thái '{statusId}' không hợp lệ. Chỉ chấp nhận: {string.Join(", ", SupplierStatusConstants.AllowedValues)}");

        RuleForEach(x => x.Ids).NotEmpty();
    }
}