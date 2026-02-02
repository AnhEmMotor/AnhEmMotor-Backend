using FluentValidation;
using System.Reflection;

namespace Application.Features.Permissions.Commands.CreateRole;

public class CreateRoleCommandValidator : AbstractValidator<CreateRoleCommand>
{
    private static readonly HashSet<string> ValidPermissions = [.. typeof(Domain.Constants.Permission.PermissionsList)
        .GetNestedTypes()
        .SelectMany(type => type.GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy))
        .Where(fieldInfo => fieldInfo.IsLiteral && !fieldInfo.IsInitOnly)
        .Select(fieldInfo => fieldInfo.GetRawConstantValue() as string)
        .Where(permission => permission is not null)
        .Cast<string>()]; 

    public CreateRoleCommandValidator()
    {
        RuleFor(x => x.RoleName)
            .NotEmpty().WithMessage("Role name is required.")
            .MaximumLength(100)
            .Matches(@"^[a-zA-Z0-9\s]*$").WithMessage("Role name cannot contain special characters."); // Fix cho PERM_014

        RuleFor(x => x.Permissions)
            .NotEmpty().WithMessage("At least one permission must be assigned.")
            .Must(permissions => permissions != null && permissions.All(p => ValidPermissions.Contains(p)))
            .WithMessage("One or more permissions are invalid."); // Fix cho PERM_013
    }
}