namespace Domain.Enums;

public static class ContactType
{
    public const string Support = "Support";
    public const string Feedback = "Feedback";
    public const string Candidate = "Candidate";

    public static readonly string[] All = [Support, Feedback, Candidate];
}
