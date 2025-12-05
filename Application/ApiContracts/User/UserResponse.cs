namespace Application.ApiContracts.User;

public class UserResponse
{
    public Guid Id { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Gender { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public bool EmailConfirmed { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTimeOffset? DeletedAt { get; set; }
    public List<string> Roles { get; set; } = [];
}