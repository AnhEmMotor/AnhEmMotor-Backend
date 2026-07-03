using System.ComponentModel.DataAnnotations;

namespace Application.ApiContracts.Contacts.Requests;

public record CreateFeedbackRequest
{
    [Required]
    [Range(1, 5)]
    public int Rating { get; set; }

    [Required]
    [MaxLength(250)]
    public string FeedbackArea { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string CustomerName { get; set; } = string.Empty;

    [Required]
    [MaxLength(20)]
    public string PhoneNumber { get; set; } = string.Empty;

    [Required]
    public string Content { get; set; } = string.Empty;
}
