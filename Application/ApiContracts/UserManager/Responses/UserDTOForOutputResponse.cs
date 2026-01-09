namespace Application.ApiContracts.UserManager.Responses;

public class UserDTOForOutputResponse
{
    public Guid? Id { get; set; }

    public string? Email { get; set; } = string.Empty;

    public string? PhoneNumber { get; set; } = string.Empty;    

    public string? FullName { get; set; } = string.Empty;
}