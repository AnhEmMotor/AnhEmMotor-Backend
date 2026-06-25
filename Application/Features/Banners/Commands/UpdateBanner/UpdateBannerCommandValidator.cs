using FluentValidation;

namespace Application.Features.Banners.Commands.UpdateBanner;

public class UpdateBannerCommandValidator : AbstractValidator<UpdateBannerCommand>
{
    public UpdateBannerCommandValidator()
    {
        RuleFor(v => v.Id).GreaterThan(0).WithMessage("Id không hợp lệ.");
        RuleFor(v => v.Title).NotEmpty().WithMessage("Tiêu đề banner là bắt buộc.");
        RuleFor(v => v.Placement).NotEmpty().WithMessage("Vị trí hiển thị là bắt buộc.");
        RuleFor(v => v.Description).NotEmpty().WithMessage("Mô tả banner là bắt buộc.");
        RuleFor(v => v.CtaLabel).NotEmpty().WithMessage("Nhãn nút bấm là bắt buộc.");
        RuleFor(v => v.CtaLink).NotEmpty().WithMessage("Đường dẫn đích là bắt buộc.");
        RuleFor(v => v)
            .Must(v => !string.IsNullOrEmpty(v.DesktopImageUrl) || !string.IsNullOrEmpty(v.MobileImageUrl))
            .WithMessage("Phải có ít nhất 1 ảnh (Desktop hoặc Mobile).");
    }
}
