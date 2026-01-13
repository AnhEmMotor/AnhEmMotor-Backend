using FluentValidation;

namespace Application.Features.Brands.Commands.DeleteManyBrands;

public sealed class DeleteManyBrandsCommandValidator : AbstractValidator<DeleteManyBrandsCommand>
{
    public DeleteManyBrandsCommandValidator()
    {
        RuleFor(x => x.Ids)
            .NotEmpty()
            .WithMessage("You must provide at least one ID.")
            .Must(ids => ids.Distinct().Count() == ids.Count)
            .WithMessage("Duplicate IDs are not allowed.");
    }
}