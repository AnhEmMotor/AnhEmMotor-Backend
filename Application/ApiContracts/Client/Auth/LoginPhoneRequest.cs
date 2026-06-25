namespace Application.ApiContracts.Client.Auth
{
    public record LoginPhoneRequest(string PhoneNumber);

    public record VerifyOtpRequest(string PhoneNumber, string OtpCode);

    public record AuthResponse(string AccessToken, string RefreshToken, DateTime Expiry);
}
