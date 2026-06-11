namespace Domain.Constants
{
    public static class BannerPlacements
    {
        public const string Home = "Home";
        public const string News = "News";
        public const string Promotion = "Promotion";

        public static readonly IReadOnlyDictionary<string, string> PlacementLabels = new Dictionary<string, string>(
            StringComparer.OrdinalIgnoreCase)
        { { Home, "Trang chủ" }, { News, "Tin tức" }, { Promotion, "Khuyến mãi" } };
    }
}
