using Domain.Constants.Product;
using FluentValidation;

namespace Application.Features.Products.Commands.UpdateProductStatus;

public class UpdateProductStatusCommandValidator : AbstractValidator<UpdateProductStatusCommand>
{
    public UpdateProductStatusCommandValidator()
    {
        RuleFor(x => x.StatusId)
            .NotEmpty()
            .WithMessage("Status is required.")
            .Must(status => ProductStatus.IsValid(status))
            .WithMessage($"Status must be one of: {string.Join(", ", ProductStatus.AllowedValues)}.");
    }
}
