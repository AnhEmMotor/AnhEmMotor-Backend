using System;

namespace Application.ApiContracts.Loyalty.Responses
{
    public class LoyaltyMemberResponse
    {
        public int Id { get; set; }

        public string FullName { get; set; } = string.Empty;

        public string PhoneNumber { get; set; } = string.Empty;

        public string Tier { get; set; } = string.Empty;

        public int Points { get; set; }
    }
}
