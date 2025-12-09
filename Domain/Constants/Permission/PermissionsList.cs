namespace Domain.Constants.Permission;

/// <summary>
/// Định nghĩa các quyền trong hệ thống với metadata (tên, mô tả)
/// </summary>
public static class PermissionsList
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
        public const string EditPrice = "Permissions.Products.EditPrice";
        public const string ChangeStatus = "Permissions.Products.ChangeStatus";
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
        public const string ChangeStatus = "Permissions.Inputs.ChangeStatus";
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
        public const string ChangeStatus = "Permissions.Outputs.ChangeStatus";
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
        public const string ChangePassword = "Permissions.Users.ChangePassword";
    }

    /// <summary>
    /// Metadata của các permissions (tên hiển thị, mô tả)
    /// </summary>
    private static readonly Dictionary<string, PermissionMetadata> PermissionMetadataMap = new()
    {
        { Brands.View, new PermissionMetadata("View Brands", "Xem danh sách thương hiệu") },
        { Brands.Create, new PermissionMetadata("Create Brand", "Tạo thương hiệu mới") },
        { Brands.Edit, new PermissionMetadata("Edit Brand", "Chỉnh sửa thương hiệu") },
        { Brands.Delete, new PermissionMetadata("Delete Brand", "Xóa thương hiệu") },

        { Products.View, new PermissionMetadata("View Products", "Xem danh sách sản phẩm") },
        { Products.Create, new PermissionMetadata("Create Product", "Tạo sản phẩm mới") },
        { Products.Edit, new PermissionMetadata("Edit Product", "Chỉnh sửa sản phẩm") },
        { Products.Delete, new PermissionMetadata("Delete Product", "Xóa sản phẩm") },
        { Products.EditPrice, new PermissionMetadata("Edit Price", "Chỉnh giá của sản phẩm") },
        { Products.ChangeStatus, new PermissionMetadata("Change Status", "Chỉnh trạng thái của sản phẩm (đang bán hoặc ngưng bán)") },

        { ProductCategories.View, new PermissionMetadata("View Product Categories", "Xem danh mục sản phẩm") },
        { ProductCategories.Create, new PermissionMetadata("Create Product Category", "Tạo danh mục sản phẩm mới") },
        { ProductCategories.Edit, new PermissionMetadata("Edit Product Category", "Chỉnh sửa danh mục sản phẩm") },
        { ProductCategories.Delete, new PermissionMetadata("Delete Product Category", "Xóa danh mục sản phẩm") },

        { Suppliers.View, new PermissionMetadata("View Suppliers", "Xem danh sách nhà cung cấp") },
        { Suppliers.Create, new PermissionMetadata("Create Supplier", "Tạo nhà cung cấp mới") },
        { Suppliers.Edit, new PermissionMetadata("Edit Supplier", "Chỉnh sửa nhà cung cấp") },
        { Suppliers.Delete, new PermissionMetadata("Delete Supplier", "Xóa nhà cung cấp") },

        { Inputs.View, new PermissionMetadata("View Inputs", "Xem danh sách đơn nhập hàng") },
        { Inputs.Create, new PermissionMetadata("Create Input", "Tạo đơn nhập hàng mới") },
        { Inputs.Edit, new PermissionMetadata("Edit Input", "Chỉnh sửa đơn nhập hàng") },
        { Inputs.Delete, new PermissionMetadata("Delete Input", "Xóa đơn nhập hàng") },
        { Inputs.ChangeStatus, new PermissionMetadata("Change Status", "Thay đổi trạng thái đơn nhập hàng (hoàn tất hoặc huỷ bỏ)") },

        { Outputs.View, new PermissionMetadata("View Outputs", "Xem danh sách đơn xuất hàng") },
        { Outputs.Create, new PermissionMetadata("Create Output", "Tạo đơn xuất hàng mới") },
        { Outputs.Edit, new PermissionMetadata("Edit Output", "Chỉnh sửa đơn xuất hàng") },
        { Outputs.Delete, new PermissionMetadata("Delete Output", "Xóa đơn xuất hàng") },
        { Outputs.ChangeStatus, new PermissionMetadata("Change Status", "Thay đổi trạng thái đơn hàng") },

        { Files.View, new PermissionMetadata("View Files", "Xem danh sách tệp tin") },
        { Files.Upload, new PermissionMetadata("Upload File", "Tải lên tệp tin") },
        { Files.Delete, new PermissionMetadata("Delete File", "Xóa tệp tin") },

        { Settings.View, new PermissionMetadata("View Settings", "Xem cài đặt hệ thống") },
        { Settings.Edit, new PermissionMetadata("Edit Settings", "Chỉnh sửa cài đặt hệ thống") },

        { Statistical.View, new PermissionMetadata("View Statistics", "Xem thống kê") },
        { Statistical.Export, new PermissionMetadata("Export Statistics", "Xuất báo cáo thống kê") },

        { Roles.View, new PermissionMetadata("View Roles", "Xem danh sách vai trò") },
        { Roles.Create, new PermissionMetadata("Create Role", "Tạo vai trò mới") },
        { Roles.Edit, new PermissionMetadata("Edit Role", "Chỉnh sửa vai trò") },
        { Roles.Delete, new PermissionMetadata("Delete Role", "Xóa vai trò") },
        { Roles.AssignPermissions, new PermissionMetadata("Assign Permissions", "Gán quyền cho vai trò") },

        { Users.View, new PermissionMetadata("View Users", "Xem danh sách người dùng") },
        { Users.Create, new PermissionMetadata("Create User", "Tạo người dùng mới") },
        { Users.Edit, new PermissionMetadata("Edit User", "Chỉnh sửa thông tin người dùng") },
        { Users.Delete, new PermissionMetadata("Delete User", "Xóa người dùng") },
        { Users.AssignRoles, new PermissionMetadata("Assign Roles", "Gán vai trò cho người dùng") },
        { Users.ChangePassword, new PermissionMetadata("Change Password", "Đổi mật khẩu người dùng")  },
    };

    /// <summary>
    /// Lấy metadata (tên hiển thị, mô tả) của một permission
    /// </summary>
    /// <param name="permissionName">Tên permission</param>
    /// <returns>Metadata chứa DisplayName và Description, hoặc null nếu không tìm thấy</returns>
    public static PermissionMetadata? GetMetadata(string permissionName)
    { return PermissionMetadataMap.TryGetValue(permissionName, out var metadata) ? metadata : null; }
}
