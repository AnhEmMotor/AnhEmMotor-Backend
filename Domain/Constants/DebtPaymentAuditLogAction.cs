namespace Domain.Constants;

public static class DebtPaymentAuditLogAction
{
    public const string Create = "Create";
    public const string Pay = "Pay";
    public const string Update = "Update";
    public const string Delete = "Delete";

    public static string Translate(string? action)
    {
        return action switch
        {
            Create => "Thêm mới",
            Pay => "Thanh toán",
            Update => "Cập nhật",
            Delete => "Xoá",
            _ => action ?? string.Empty
        };
    }
}
