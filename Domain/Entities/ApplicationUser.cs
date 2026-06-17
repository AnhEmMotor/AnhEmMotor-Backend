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

    public DateTimeOffset? CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    public string? AvatarUrl { get; set; }

    public DateTime? DateOfBirth { get; set; }

    public ICollection<IdentityUserRole<Guid>> UserRoles { get; set; } = [];

    public ICollection<InventoryTransaction> InventoryTransactions { get; set; } = [];

    public ICollection<OrderStatusHistory> OrderStatusHistories { get; set; } = [];

    public ICollection<CustomerContact> ProcessedContacts { get; set; } = [];

    public ICollection<CustomerContactReply> ContactReplies { get; set; } = [];

    public ICollection<BookingAppointment> ConfirmedBookings { get; set; } = [];

    public ICollection<NewsArticle> AuthoredNewsArticles { get; set; } = [];

    public ICollection<NewsArticle> PublishedNewsArticles { get; set; } = [];

    public ICollection<PromotionBanner> CreatedBanners { get; set; } = [];

    public ICollection<PromotionBanner> UpdatedBanners { get; set; } = [];
}
