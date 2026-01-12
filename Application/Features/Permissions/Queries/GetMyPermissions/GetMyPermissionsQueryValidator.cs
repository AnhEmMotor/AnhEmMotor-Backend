using FluentValidation;

namespace Application.Features.Permissions.Queries.GetMyPermissions;
public sealed class GetMyPermissionsQueryValidator : AbstractValidator<GetMyPermissionsQuery>
{
    public GetMyPermissionsQueryValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("User ID is required.")
            .Must(BeAValidGuid).WithMessage("Invalid User ID format.");
    }

    private static bool BeAValidGuid(string userId)
    {
        return Guid.TryParse(userId, out _);
    }
}