using Domain.Constants.Permission;
using FluentValidation;
using System.Reflection;

namespace Application.Features.Permissions.Commands.CreateRole;

public class CreateRoleCommandValidator : AbstractValidator<CreateRoleCommand>
{
    private static readonly HashSet<string> ValidPermissions = [.. typeof(PermissionsList)
        .GetNestedTypes()
        .SelectMany(type => type.GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy))
        .Where(fieldInfo => fieldInfo.IsLiteral && !fieldInfo.IsInitOnly)
        .Select(fieldInfo => fieldInfo.GetRawConstantValue() as string)
        .Where(permission => permission is not null)
        .Cast<string>()];

    public CreateRoleCommandValidator()
    {
        RuleFor(x => x.RoleName)
            .NotEmpty()
            .WithMessage("Role name is required.")
            .MaximumLength(100)
            .Matches(@"^[\p{L}0-9\s_\-]*$")
            .WithMessage("Role name can only contain letters, numbers, spaces, underscores, and hyphens.");
        RuleFor(x => x.Permissions)
            .NotEmpty()
            .WithMessage("At least one permission must be assigned.")
            .Must(permissions => permissions != null && permissions.All(p => ValidPermissions.Contains(p)))
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