using Domain.Constants;
using FluentValidation;

namespace Application.Features.Products.Commands.UpdateManyProductStatuses;

public sealed class UpdateManyProductStatusesCommandValidator : AbstractValidator<UpdateManyProductStatusesCommand>
{
    public UpdateManyProductStatusesCommandValidator()
    {
        RuleFor(x => x.Ids).NotEmpty().WithMessage("Ids list cannot be empty.");

        RuleForEach(x => x.Ids).GreaterThan(0).WithMessage("Each Id must be greater than 0.");

        RuleFor(x => x.StatusId)
            .NotEmpty()
            .WithMessage("Status is required.")
            .Must(status => ProductStatus.IsValid(status))
            .WithMessage($"Status must be one of: {string.Join(", ", ProductStatus.AllowedValues)}.");
    }
}
