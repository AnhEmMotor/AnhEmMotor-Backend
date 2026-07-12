namespace Application.ApiContracts.Auth.Responses;

public class ResetPasswordResponse
{
    public bool Success { get; set; }

    public string Message { get; set; } = string.Empty;
}
