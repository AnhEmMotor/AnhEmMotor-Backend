using System;

namespace Domain.Constants
{
    public static class QuotationType
    {
        public const string Draft = "draft";
        public const string Sent = "sent";
        public const string Approved = "approved";
        public const string Rejected = "rejected";

        public static readonly HashSet<string> All = [Draft, Sent, Approved, Rejected];

        public static string GetDisplayName(string status) => status.ToLower() switch
        {
            "draft" => "Phi?u t?m",
            "sent" => "ау g?i",
            "approved" => "ау duy?t",
            "rejected" => "ау t? ch?i",
            _ => status
        };

        public static IEnumerable<object> GetActiveList()
        {
            return All.Select(type => new { Value = type, Text = GetDisplayName(type) });
        }
    }
}
