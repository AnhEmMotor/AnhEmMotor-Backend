namespace Domain.Constants;

/// <summary>
/// Định nghĩa các quyền trong hệ thống
/// </summary>
public static class Permissions
{
    /// <summary>
    /// Quyền quản lý thương hiệu
    /// </summary>
    public static class Brands
    {
        public const string View = "Permissions.Brands.View";
        public const string Create = "Permissions.Brands.Create";
        public const string Edit = "Permissions.Brands.Edit";
        public const string Delete = "Permissions.Brands.Delete";
    }

    /// <summary>
    /// Quyền quản lý sản phẩm
    /// </summary>
    public static class Products
    {
        public const string View = "Permissions.Products.View";
        public const string Create = "Permissions.Products.Create";
        public const string Edit = "Permissions.Products.Edit";
        public const string Delete = "Permissions.Products.Delete";
    }

    /// <summary>
    /// Quyền quản lý danh mục sản phẩm
    /// </summary>
    public static class ProductCategories
    {
        public const string View = "Permissions.ProductCategories.View";
        public const string Create = "Permissions.ProductCategories.Create";
        public const string Edit = "Permissions.ProductCategories.Edit";
        public const string Delete = "Permissions.ProductCategories.Delete";
    }

    /// <summary>
    /// Quyền quản lý nhà cung cấp
    /// </summary>
    public static class Suppliers
    {
        public const string View = "Permissions.Suppliers.View";
        public const string Create = "Permissions.Suppliers.Create";
        public const string Edit = "Permissions.Suppliers.Edit";
        public const string Delete = "Permissions.Suppliers.Delete";
    }

    /// <summary>
    /// Quyền quản lý đơn nhập hàng
    /// </summary>
    public static class Inputs
    {
        public const string View = "Permissions.Inputs.View";
        public const string Create = "Permissions.Inputs.Create";
        public const string Edit = "Permissions.Inputs.Edit";
        public const string Delete = "Permissions.Inputs.Delete";
        public const string Approve = "Permissions.Inputs.Approve";
    }

    /// <summary>
    /// Quyền quản lý đơn xuất hàng
    /// </summary>
    public static class Outputs
    {
        public const string View = "Permissions.Outputs.View";
        public const string Create = "Permissions.Outputs.Create";
        public const string Edit = "Permissions.Outputs.Edit";
        public const string Delete = "Permissions.Outputs.Delete";
        public const string Approve = "Permissions.Outputs.Approve";
    }

    /// <summary>
    /// Quyền quản lý tệp tin
    /// </summary>
    public static class Files
    {
        public const string View = "Permissions.Files.View";
        public const string Upload = "Permissions.Files.Upload";
        public const string Delete = "Permissions.Files.Delete";
    }

    /// <summary>
    /// Quyền quản lý cài đặt
    /// </summary>
    public static class Settings
    {
        public const string View = "Permissions.Settings.View";
        public const string Edit = "Permissions.Settings.Edit";
    }

    /// <summary>
    /// Quyền xem thống kê
    /// </summary>
    public static class Statistical
    {
        public const string View = "Permissions.Statistical.View";
        public const string Export = "Permissions.Statistical.Export";
    }

    /// <summary>
    /// Quyền quản lý vai trò và phân quyền
    /// </summary>
    public static class Roles
    {
        public const string View = "Permissions.Roles.View";
        public const string Create = "Permissions.Roles.Create";
        public const string Edit = "Permissions.Roles.Edit";
        public const string Delete = "Permissions.Roles.Delete";
        public const string AssignPermissions = "Permissions.Roles.AssignPermissions";
    }

    /// <summary>
    /// Quyền quản lý người dùng
    /// </summary>
    public static class Users
    {
        public const string View = "Permissions.Users.View";
        public const string Create = "Permissions.Users.Create";
        public const string Edit = "Permissions.Users.Edit";
        public const string Delete = "Permissions.Users.Delete";
        public const string AssignRoles = "Permissions.Users.AssignRoles";
    }
}
