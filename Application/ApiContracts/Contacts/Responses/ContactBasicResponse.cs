using System.Collections.Generic;

namespace Application.ApiContracts.Contacts.Responses;

public record ContactBasicResponse
{
    public int Id { get; set; }

    public string FullName { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string PhoneNumber { get; set; } = string.Empty;

    public string? InternalNote { get; set; }

    public DateTimeOffset? CreatedAt { get; set; }

    public List<ContactReplyResponse> Replies { get; set; } = [];
}
