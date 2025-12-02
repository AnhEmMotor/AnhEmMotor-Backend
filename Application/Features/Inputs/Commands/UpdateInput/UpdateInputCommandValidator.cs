using FluentValidation;

namespace Application.Features.Inputs.Commands.CreateInput;

public sealed class UpdateInputCommandValidator : AbstractValidator<CreateInputCommand>
{
    public UpdateInputCommandValidator()
    {
        RuleFor(x => x.Products)
            .NotEmpty()
            .WithMessage("Input must contain at least one product.");

        RuleFor(x => x.Products)
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
            .WithMessage("Product ID cannot be duplicated in a single input.");

        RuleForEach(x => x.Products).SetValidator(new UpdateInputProductCommandValidator());
    }
}

public sealed class UpdateInputProductCommandValidator : AbstractValidator<CreateInputProductCommand>
{
    public UpdateInputProductCommandValidator()
    {
        RuleFor(x => x.ProductId)
            .NotNull()
            .GreaterThan(0);

        RuleFor(x => x.Count)
            .NotNull()
            .GreaterThan((short)0);

        RuleFor(x => x.InputPrice)
            .NotNull()
            .GreaterThanOrEqualTo(0);
    }
}