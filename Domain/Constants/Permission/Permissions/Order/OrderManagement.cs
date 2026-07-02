namespace Domain.Constants.Permission;

public static partial class Permissions
{
    public static partial class Order 
    {
        public static class OrderManagement 
        {
            public const string View = "Permissions.Order.OrderManagement.View";
            public const string Create = "Permissions.Order.OrderManagement.Create";
            public const string Edit = "Permissions.Order.OrderManagement.Edit";
            public const string Delete = "Permissions.Order.OrderManagement.Delete";
            public const string ChangeStatus = "Permissions.Order.OrderManagement.ChangeStatus";
        }
    }
}
