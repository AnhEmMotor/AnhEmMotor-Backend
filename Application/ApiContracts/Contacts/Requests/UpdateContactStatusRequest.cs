using System.ComponentModel.DataAnnotations;

namespace Application.ApiContracts.Contacts.Requests;

public record UpdateContactStatusRequest
{
    [Required]
    [MaxLength(20)]
    public string Status { get; set; } = string.Empty;
}
