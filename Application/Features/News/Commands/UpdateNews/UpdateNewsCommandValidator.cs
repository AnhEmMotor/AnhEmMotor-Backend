using FluentValidation;

namespace Application.Features.News.Commands.UpdateNews;

public class UpdateNewsCommandValidator : AbstractValidator<UpdateNewsCommand>
{
    public UpdateNewsCommandValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty()
            .WithMessage("Tiêu đề không được để trống.")
            .MaximumLength(255)
            .WithMessage("Tiêu đề không được vượt quá 255 ký tự.");
        RuleFor(x => x.Slug)
            .NotEmpty()
            .WithMessage("Đường dẫn tĩnh không được để trống.")
            .MaximumLength(255)
            .WithMessage("Slug không được vượt quá 255 ký tự.");
        RuleFor(x => x.CategoryId)
            .NotEmpty()
            .WithMessage("Danh mục bài viết không được để trống.")
            .GreaterThan(0)
            .WithMessage("Danh mục bài viết không hợp lệ.");
        RuleFor(x => x.CoverImageUrl).NotEmpty().WithMessage("Ảnh bìa là bắt buộc.");
        RuleFor(x => x.Content).NotEmpty().WithMessage("Nội dung bài viết không được để trống.");
        RuleFor(x => x.MetaTitle).MaximumLength(100).WithMessage("Tiêu đề SEO không được vượt quá 100 ký tự.");
        RuleFor(x => x.MetaDescription).MaximumLength(255).WithMessage("Mô tả SEO không được vượt quá 255 ký tự.");
    }
}
