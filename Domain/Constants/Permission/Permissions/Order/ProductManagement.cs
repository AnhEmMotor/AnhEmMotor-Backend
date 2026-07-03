namespace Domain.Constants.Permission;

public static partial class Permissions
{
    public static partial class Order 
    {
        public static class ProductManagement 
        {
            public const string View = "Permissions.Order.ProductManagement.View";
            public const string Create = "Permissions.Order.ProductManagement.Create";
            public const string Edit = "Permissions.Order.ProductManagement.Edit";
            public const string Delete = "Permissions.Order.ProductManagement.Delete";
            public const string EditPrice = "Permissions.Order.ProductManagement.EditPrice";
            public const string ChangeStatus = "Permissions.Order.ProductManagement.ChangeStatus";
        }
    }
}
