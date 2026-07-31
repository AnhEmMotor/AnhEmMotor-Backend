namespace Domain.Constants;

public static class ChatPlanStatus
{
    public const string Drafting = "Drafting";   // AI đang sinh, user sửa được
    public const string Ready = "Ready";         // AI sinh xong, chờ duyệt
    public const string Approved = "Approved";   // đã duyệt, chuẩn bị thực thi
    public const string Executing = "Executing";
    public const string Completed = "Completed";
    public const string Rejected = "Rejected";   // user huỷ hoặc hết hạn 24h
}
