using Domain.Constants.Product;
using FluentValidation;

namespace Application.Features.Products.Commands.UpdateManyProductStatuses;

public class UpdateManyProductStatusesCommandValidator : AbstractValidator<UpdateManyProductStatusesCommand>
{
    public UpdateManyProductStatusesCommandValidator()
    {
        RuleFor(x => x.Ids).NotEmpty().WithMessage("Ids list cannot be empty.");
        RuleFor(x => x.StatusId)
            .NotEmpty()
            .WithMessage("Status is required.")
            .Must(status => ProductStatus.IsValid(status))
            .WithMessage($"Status must be one of: {string.Join(", ", ProductStatus.AllowedValues)}.");
    }
}
