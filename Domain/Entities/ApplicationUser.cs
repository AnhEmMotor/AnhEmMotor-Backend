using Domain.Constants;
using Microsoft.AspNetCore.Identity;

namespace Domain.Entities;

public class ApplicationUser : IdentityUser<Guid>
{
    public string FullName { get; set; } = string.Empty;

    public string Gender { get; set; } = GenderStatus.Male;

    public string? RefreshToken { get; set; }

    public DateTimeOffset RefreshTokenExpiryTime { get; set; }

    public string Status { get; set; } = UserStatus.Active;

    public DateTimeOffset? DeletedAt { get; set; }

    public ICollection<IdentityUserRole<Guid>> UserRoles { get; set; } = [];
}
