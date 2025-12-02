using FluentValidation;
using Application.Features.Inputs.Commands.CreateInput;
using Application;
using Application.Features;
using Application.Features.Inputs;
using Application.Features.Inputs.Commands;
using Application.ApiContracts.Output;

namespace Application.Features.Outputs.Commands.UpdateOutput;

public sealed class UpdateOutputCommandValidator : AbstractValidator<UpdateOutputCommand>
{
    public UpdateOutputCommandValidator()
    {
        RuleFor(x => x.OutputInfos)
            .NotEmpty()
            .WithMessage("Input must contain at least one product.");

        RuleFor(x => x.OutputInfos)
            .Must(products =>
            {
                // Bước 1: Lấy ra tất cả ProductId hợp lệ
                var productIds = products
                    .Where(p => p.ProductId.HasValue) // Chỉ xem xét các sản phẩm đã có ID
                    .Select(p => p!.ProductId!.Value)
                    .ToList();

                // Bước 2: Đếm số lượng ProductId khác nhau
                var distinctCount = productIds.Distinct().Count();

                // Bước 3: So sánh số lượng khác nhau với tổng số ID đã đếm
                // Nếu tổng số ID (productIds.Count) KHÁC với số lượng ID duy nhất (distinctCount),
                // nghĩa là có sự lặp lại.
                var isDuplicate = productIds.Count != distinctCount;

                // Trả về TRUE (PASS) nếu KHÔNG có lặp lại.
                return !isDuplicate;
            })
            .WithMessage("Product ID cannot be duplicated in a single output.");

        RuleForEach(x => x.OutputInfos).SetValidator(new UpdateOutputProductCommandValidator());
    }
}

public sealed class UpdateOutputProductCommandValidator : AbstractValidator<OutputInfoDto>
{
    public UpdateOutputProductCommandValidator()
    {
        RuleFor(x => x.ProductId)
            .NotNull()
            .GreaterThan(0);

        RuleFor(x => x.Count)
            .NotNull()
            .GreaterThan((short)0);
    }
}