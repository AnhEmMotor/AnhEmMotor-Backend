using FluentValidation;

namespace Application.Features.UserManager.Commands.AssignRoles;

public sealed class AssignRolesCommandValidator : AbstractValidator<AssignRolesCommand>
{
    public AssignRolesCommandValidator()
    {
        RuleFor(x => x.UserId).NotEmpty().WithMessage("UserId is required.");

        RuleFor(x => x.Model.RoleNames)
            .NotNull().WithMessage("Role list cannot be null.")
            .Must(roles => roles.Count == roles.Distinct(StringComparer.OrdinalIgnoreCase).Count())
            .WithMessage("Duplicate roles found in request.");

        RuleForEach(x => x.Model.RoleNames)
            .NotEmpty().WithMessage("Role name cannot be empty.");
    }
}