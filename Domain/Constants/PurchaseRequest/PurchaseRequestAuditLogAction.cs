namespace Domain.Constants.PurchaseRequest;

public static class PurchaseRequestAuditLogAction
{
    public const string Add = "Add";
    public const string Update = "Update";
    public const string Delete = "Delete";
    public const string UpdateStatus = "UpdateStatus";

    public static string Translate(string? action)
    {
        return action switch
        {
            Add => "Thêm mới",
            Update => "Cập nhật",
            Delete => "Xoá",
            UpdateStatus => "Cập nhật trạng thái",
            _ => action ?? string.Empty
        };
    }
}
