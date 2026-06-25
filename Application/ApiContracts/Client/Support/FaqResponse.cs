namespace Application.ApiContracts.Client.Support
{
    public record FaqResponse(string Question, string Answer);

    public record CallbackRequest(string PhoneNumber, string IssueDescription);

    public record FeedbackRequest(int Rating, string Comment, List<string> MediaUrls);
}
