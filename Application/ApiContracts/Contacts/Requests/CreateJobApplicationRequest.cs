using System.ComponentModel.DataAnnotations;

namespace Application.ApiContracts.Contacts.Requests;

public record CreateJobApplicationRequest
{
    [Required]
    [MaxLength(100)]
    public string FullName { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    [MaxLength(20)]
    public string PhoneNumber { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string AppliedPosition { get; set; } = string.Empty;

    [MaxLength(500)]
    public string CvFileUrl { get; set; } = string.Empty;

    public string? CoverLetter { get; set; }
}
