namespace Domain.Shared
{
    public class PagedResult<T>(List<T>? items, long? totalCount, int? pageNumber, int? pageSize)
    {
        public List<T>? Items { get; set; } = items;

        public int? PageNumber { get; set; } = pageNumber;

        public int? PageSize { get; set; } = pageSize;

        public int? TotalPages { get; set; } = CalculateTotalPages(totalCount, pageSize);

        public long? TotalCount { get; set; } = totalCount;

        private static int? CalculateTotalPages(long? totalCount, int? pageSize)
        {
            if(totalCount == null || pageSize == null || pageSize.Value <= 0)
            {
                return null;
            }
            double count = totalCount.Value;
            double size = pageSize.Value;
            return (int)Math.Ceiling(count / size);
        }
    }
}