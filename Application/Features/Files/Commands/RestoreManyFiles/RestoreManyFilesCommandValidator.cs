using FluentValidation;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Features.Files.Commands.RestoreManyFiles;

public sealed class RestoreManyFilesCommandValidator : AbstractValidator<RestoreManyFilesCommand>
{
    public RestoreManyFilesCommandValidator()
    {
        RuleFor(x => x.StoragePaths)
            .NotEmpty().WithMessage("You must provide at least one storage path.")
            .Must(paths => paths.Count <= 20) // Giới hạn số lượng gắt hơn vì query string nặng
            .WithMessage("You cannot restore more than 20 files at once.");

        RuleForEach(x => x.StoragePaths)
            .NotEmpty().WithMessage("Storage path cannot be empty.");
    }
}