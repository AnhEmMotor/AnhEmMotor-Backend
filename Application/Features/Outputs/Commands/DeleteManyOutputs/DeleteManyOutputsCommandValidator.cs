using FluentValidation;

namespace Application.Features.Outputs.Commands.DeleteManyOutputs;

public sealed class DeleteManyOutputsCommandValidator : AbstractValidator<DeleteManyOutputsCommand>
{
    public DeleteManyOutputsCommandValidator()
    {
        RuleFor(x => x.Ids)
            .NotEmpty().WithMessage("Ids list cannot be empty.")
            .Must(ids => ids != null && ids.Count <= 100).WithMessage("Maximum 100 items per request.");
    }
}