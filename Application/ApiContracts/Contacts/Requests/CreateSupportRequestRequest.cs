using System.ComponentModel.DataAnnotations;

namespace Application.ApiContracts.Contacts.Requests;

public record CreateSupportRequestRequest
{
    [Required]
    [MaxLength(200)]
    public string Subject { get; set; } = string.Empty;

    [Required]
    [MaxLength(50)]
    public string Category { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [MaxLength(50)]
    public string? OrderCode { get; set; }

    [Required]
    public string Content { get; set; } = string.Empty;
}
