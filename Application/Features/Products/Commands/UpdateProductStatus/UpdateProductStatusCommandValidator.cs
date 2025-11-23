using Domain.Constants;
using FluentValidation;

namespace Application.Features.Products.Commands.UpdateProductStatus;

public sealed class UpdateProductStatusCommandValidator : AbstractValidator<UpdateProductStatusCommand>
{
    public UpdateProductStatusCommandValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0).WithMessage("Product Id must be greater than 0.");

        RuleFor(x => x.StatusId)
            .NotEmpty().WithMessage("Status is required.")
            .Must(status => ProductStatus.IsValid(status))
            .WithMessage($"Status must be one of: {string.Join(", ", ProductStatus.AllowedValues)}.");
    }
}
