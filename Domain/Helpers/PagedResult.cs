namespace Domain.Helpers
{
    public class PagedResult<T>(List<T> items, long totalCount, int pageNumber, int pageSize)
    {
        public List<T> Items { get; set; } = items;
        public int PageNumber { get; set; } = pageNumber;
        public int PageSize { get; set; } = pageSize;
        public int TotalPages { get; set; } = (int)Math.Ceiling((double)totalCount / pageSize);
        public long TotalCount { get; set; } = totalCount;
    }
}
