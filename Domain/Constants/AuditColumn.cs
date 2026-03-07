namespace Domain.Constants
{
    [Flags]
    public enum AuditColumn
    {
        None = 0,
        CreatedAt = 1,
        UpdatedAt = 2,
        DeletedAt = 4,
        All = CreatedAt | UpdatedAt | DeletedAt
    }
}
