namespace Application.ApiContracts.Client.Profile
{
    public record ProfileResponse(
        string FullName,
        string PhoneNumber,
        string Email,
        string MembershipGrade,
        int LoyaltyPoints,
        string DefaultRescueAddress,
        string EmergencyContactPhone);

    public record UpdateProfileRequest(
        string FullName,
        string Email,
        string DefaultRescueAddress,
        string EmergencyContactPhone);
}
