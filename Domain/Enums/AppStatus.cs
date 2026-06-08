namespace Domain.Enums;

public static class AppStatus
{
    public const string New = "New";
    public const string Interview = "Interview";
    public const string Offer = "Offer";
    public const string Rejected = "Rejected";

    public static readonly string[] All = [New, Interview, Offer, Rejected];
}
