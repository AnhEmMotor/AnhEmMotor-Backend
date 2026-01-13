using FluentValidation;
using System;

namespace Application.Features.Files.Commands.RestoreManyFiles;

public sealed class RestoreManyFilesCommandValidator : AbstractValidator<RestoreManyFilesCommand>
{
    public RestoreManyFilesCommandValidator()
    {
        RuleFor(x => x.StoragePaths)
            .NotEmpty()
            .WithMessage("You must provide at least one storage path.")
            .Must(paths => paths.Count <= 20)
            .WithMessage("You cannot restore more than 20 files at once.");

        RuleForEach(x => x.StoragePaths).NotEmpty().WithMessage("Storage path cannot be empty.");
    }
}