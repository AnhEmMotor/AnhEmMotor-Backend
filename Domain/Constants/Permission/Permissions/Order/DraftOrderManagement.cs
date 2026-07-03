namespace Domain.Constants.Permission;

public static partial class Permissions
{
    public static partial class Order 
    {
        public static class DraftOrderManagement 
        {
            public const string View = "Permissions.Order.DraftOrderManagement.View";
            public const string Create = "Permissions.Order.DraftOrderManagement.Create";
            public const string Edit = "Permissions.Order.DraftOrderManagement.Edit";
            public const string Delete = "Permissions.Order.DraftOrderManagement.Delete";
            public const string ChangeStatus = "Permissions.Order.DraftOrderManagement.ChangeStatus";
        }
    }
}
