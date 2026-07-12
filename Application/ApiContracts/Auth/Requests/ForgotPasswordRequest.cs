namespace Application.ApiContracts.Auth.Requests;

public class ForgotPasswordRequest
{
    public string Email { get; set; } = string.Empty;
}
