using FluentValidation;

namespace Application.Features.Outputs.Commands.RestoreManyOutputs;

public sealed class RestoreManyOutputsCommandValidator : AbstractValidator<RestoreManyOutputsCommand>
{
    public RestoreManyOutputsCommandValidator()
    {
        RuleFor(x => x.Ids)
            .NotEmpty().WithMessage("Ids list cannot be empty.")
            .Must(ids => ids != null && ids.Count <= 100).WithMessage("Maximum 100 items per request.");
    }
}