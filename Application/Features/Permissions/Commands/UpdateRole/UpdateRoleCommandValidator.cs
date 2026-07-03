using Domain.Constants.Permission;
using FluentValidation;

namespace Application.Features.Permissions.Commands.UpdateRole;

public class UpdateRoleCommandValidator : AbstractValidator<UpdateRoleCommand>
{
    private static readonly HashSet<string> ValidPermissions = [.. PermissionsList.GetAllPermissions()];

    public UpdateRoleCommandValidator()
    {
        RuleFor(x => x.RoleId).NotEmpty().WithMessage("Role ID is required.");
        RuleFor(x => x.Permissions)
            .Must(permissions => permissions == null || permissions.All(p => ValidPermissions.Contains(p)))
            .WithMessage("One or more permissions are invalid.")
            .Custom(
                (permissions, context) =>
                {
                    if (permissions == null)
                        return;
                    var (isValid, errorMessage) = PermissionsList.ValidateRules(permissions);
                    if (!isValid)
                    {
                        context.AddFailure(errorMessage);
                    }
                });
    }
}
