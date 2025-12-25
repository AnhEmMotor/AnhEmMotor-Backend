namespace Application.ApiContracts.UserManager.Responses;

public class UserDTOForOutputResponse
{
    public Guid? Id { get; set; }

    public string? FullName { get; set; } = string.Empty;
}