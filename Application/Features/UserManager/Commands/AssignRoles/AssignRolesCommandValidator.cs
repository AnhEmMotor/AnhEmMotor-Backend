using FluentValidation;

namespace Application.Features.UserManager.Commands.AssignRoles;

public sealed class AssignRolesCommandValidator : AbstractValidator<AssignRolesCommand>
{
    public AssignRolesCommandValidator()
    {
        RuleFor(x => x.UserId).NotEmpty().WithMessage("UserId is required.");
        RuleFor(x => x.RoleIds)
            .NotNull()
            .WithMessage("Role list cannot be null.")
            .Must(ids => ids!.Count == ids.Distinct().Count())
            .WithMessage("Duplicate roles found in request.");
        RuleForEach(x => x.RoleIds).NotEmpty().WithMessage("Role ID cannot be empty.");
    }
}
