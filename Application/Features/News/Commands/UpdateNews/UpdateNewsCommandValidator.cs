using FluentValidation;

namespace Application.Features.News.Commands.UpdateNews;

public sealed class UpdateNewsCommandValidator : AbstractValidator<UpdateNewsCommand>
{
    public UpdateNewsCommandValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty()
            .WithMessage("Tiêu đề không được để trống.")
            .MaximumLength(255)
            .WithMessage("Tiêu đề không được vượt quá 255 ký tự.");
        RuleFor(x => x.CoverImageUrl)
            .NotEmpty()
            .WithMessage("Ảnh bìa là bắt buộc.");
        RuleFor(x => x.MetaTitle).MaximumLength(100).WithMessage("Tiêu đề SEO không được vượt quá 100 ký tự.");
        RuleFor(x => x.MetaDescription).MaximumLength(255).WithMessage("Mô tả SEO không được vượt quá 255 ký tự.");
        RuleFor(x => x.Slug).MaximumLength(255).WithMessage("Slug không được vượt quá 255 ký tự.");
    }
}
