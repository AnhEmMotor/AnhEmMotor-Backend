
namespace Domain.Constants.Permission;

public static class PermissionsList
{
    public static class Brands
    {
        public const string View = "Permissions.Brands.View";
        public const string Create = "Permissions.Brands.Create";
        public const string Edit = "Permissions.Brands.Edit";
        public const string Delete = "Permissions.Brands.Delete";
    }

    public static class Products
    {
        public const string View = "Permissions.Products.View";
        public const string Create = "Permissions.Products.Create";
        public const string Edit = "Permissions.Products.Edit";
        public const string Delete = "Permissions.Products.Delete";
        public const string EditPrice = "Permissions.Products.EditPrice";
        public const string ChangeStatus = "Permissions.Products.ChangeStatus";
    }

    public static class ProductCategories
    {
        public const string View = "Permissions.ProductCategories.View";
        public const string Create = "Permissions.ProductCategories.Create";
        public const string Edit = "Permissions.ProductCategories.Edit";
        public const string Delete = "Permissions.ProductCategories.Delete";
    }

    public static class Suppliers
    {
        public const string View = "Permissions.Suppliers.View";
        public const string Create = "Permissions.Suppliers.Create";
        public const string Edit = "Permissions.Suppliers.Edit";
        public const string Delete = "Permissions.Suppliers.Delete";
    }

    public static class Inputs
    {
        public const string View = "Permissions.Inputs.View";
        public const string Create = "Permissions.Inputs.Create";
        public const string Edit = "Permissions.Inputs.Edit";
        public const string Delete = "Permissions.Inputs.Delete";
        public const string ChangeStatus = "Permissions.Inputs.ChangeStatus";
    }

    public static class Outputs
    {
        public const string View = "Permissions.Outputs.View";
        public const string Create = "Permissions.Outputs.Create";
        public const string Edit = "Permissions.Outputs.Edit";
        public const string Delete = "Permissions.Outputs.Delete";
        public const string ChangeStatus = "Permissions.Outputs.ChangeStatus";
    }

    public static class Files
    {
        public const string View = "Permissions.Files.View";
        public const string Upload = "Permissions.Files.Upload";
        public const string Delete = "Permissions.Files.Delete";
    }

    public static class Settings
    {
        public const string View = "Permissions.Settings.View";
        public const string Edit = "Permissions.Settings.Edit";
    }

    public static class Statistical
    {
        public const string View = "Permissions.Statistical.View";
        public const string Export = "Permissions.Statistical.Export";
    }

    public static class Roles
    {
        public const string View = "Permissions.Roles.View";
        public const string Create = "Permissions.Roles.Create";
        public const string Edit = "Permissions.Roles.Edit";
        public const string Delete = "Permissions.Roles.Delete";
    }

    public static class Users
    {
        public const string View = "Permissions.Users.View";
        public const string Create = "Permissions.Users.Create";
        public const string Edit = "Permissions.Users.Edit";
        public const string Delete = "Permissions.Users.Delete";
        public const string AssignRoles = "Permissions.Users.AssignRoles";
        public const string ChangePassword = "Permissions.Users.ChangePassword";
    }

    public static class News
    {
        public const string View = "Permissions.News.View";
        public const string Create = "Permissions.News.Create";
        public const string Edit = "Permissions.News.Edit";
        public const string Delete = "Permissions.News.Delete";
    }

    public static class Banners
    {
        public const string View = "Permissions.Banners.View";
        public const string Create = "Permissions.Banners.Create";
        public const string Edit = "Permissions.Banners.Edit";
        public const string Delete = "Permissions.Banners.Delete";
    }

    public static class Contacts
    {
        public const string View = "Permissions.Contacts.View";
        public const string Reply = "Permissions.Contacts.Reply";
        public const string EditNote = "Permissions.Contacts.EditNote";
        public const string Delete = "Permissions.Contacts.Delete";
    }

    public static class Bookings
    {
        public const string View = "Permissions.Bookings.View";
        public const string Confirm = "Permissions.Bookings.Confirm";
        public const string Delete = "Permissions.Bookings.Delete";
    }

    public static class Leads
    {
        public const string View = "Permissions.Leads.View";
        public const string Create = "Permissions.Leads.Create";
        public const string Edit = "Permissions.Leads.Edit";
        public const string Delete = "Permissions.Leads.Delete";
    }

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
        {
            Products.ChangeStatus,
            new PermissionMetadata("Change Status", "Chỉnh trạng thái của sản phẩm (đang bán hoặc ngưng bán)")
        },

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
        {
            Inputs.ChangeStatus,
            new PermissionMetadata("Change Status", "Thay đổi trạng thái đơn nhập hàng (hoàn tất hoặc huỷ bỏ)")
        },

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

        { Users.View, new PermissionMetadata("View Users", "Xem danh sách người dùng") },
        { Users.Create, new PermissionMetadata("Create User", "Tạo người dùng mới") },
        { Users.Edit, new PermissionMetadata("Edit User", "Chỉnh sửa thông tin người dùng") },
        { Users.Delete, new PermissionMetadata("Delete User", "Xóa người dùng") },
        { Users.AssignRoles, new PermissionMetadata("Assign Roles", "Gán vai trò cho người dùng") },
        { Users.ChangePassword, new PermissionMetadata("Change Password", "Đổi mật khẩu người dùng") },

        { News.View, new PermissionMetadata("View News", "Xem danh sách tin tức") },
        { News.Create, new PermissionMetadata("Create News", "Tạo tin tức mới") },
        { News.Edit, new PermissionMetadata("Edit News", "Chỉnh sửa tin tức") },
        { News.Delete, new PermissionMetadata("Delete News", "Xóa tin tức") },

        { Banners.View, new PermissionMetadata("View Banners", "Xem danh sách banner") },
        { Banners.Create, new PermissionMetadata("Create Banner", "Thêm banner mới") },
        { Banners.Edit, new PermissionMetadata("Edit Banner", "Chỉnh sửa banner") },
        { Banners.Delete, new PermissionMetadata("Delete Banner", "Xóa banner") },

        { Contacts.View, new PermissionMetadata("View Contacts", "Xem danh sách liên hệ khách hàng") },
        { Contacts.Reply, new PermissionMetadata("Reply to Contact", "Phản hồi liên hệ khách hàng") },
        { Contacts.EditNote, new PermissionMetadata("Edit Internal Note", "Chỉnh sửa ghi chú nội bộ khách hàng") },
        { Contacts.Delete, new PermissionMetadata("Delete Contact", "Xóa liên hệ khách hàng") },

        { Bookings.View, new PermissionMetadata("View Bookings", "Xem danh sách đặt lịch lái thử") },
        { Bookings.Confirm, new PermissionMetadata("Confirm Booking", "Xác nhận lịch hẹn lái thử") },
        { Bookings.Delete, new PermissionMetadata("Delete Booking", "Xóa lịch hẹn lái thử") },

        { Leads.View, new PermissionMetadata("View Leads", "Xem danh sách khách hàng tiềm năng") },
        { Leads.Create, new PermissionMetadata("Create Lead", "Tạo khách hàng tiềm năng") },
        { Leads.Edit, new PermissionMetadata("Edit Lead", "Chỉnh sửa khách hàng tiềm năng") },
        { Leads.Delete, new PermissionMetadata("Delete Lead", "Xóa khách hàng tiềm năng") },
    };

    public static PermissionMetadata? GetMetadata(string permissionName)
    {
        return PermissionMetadataMap.TryGetValue(permissionName, out var metadata) ? metadata : null;
    }

    public static IEnumerable<(string Id, string Name, string Description)> GetMetadataList()
    {
        return PermissionMetadataMap.Select(
            kv => (
            kv.Key,
            kv.Value.DisplayName,
            kv.Value.Description
        ));
    }

    public static readonly Dictionary<string, List<string>> Groups = new()
    {
        { "Thương hiệu", [Brands.View, Brands.Create, Brands.Edit, Brands.Delete] },
        {
            "Sản phẩm",
            [Products.View, Products.Create, Products.Edit, Products.Delete, Products.EditPrice, Products.ChangeStatus]
        },
        {
            "Danh mục sản phẩm",
            [ProductCategories.View, ProductCategories.Create, ProductCategories.Edit, ProductCategories.Delete]
        },
        { "Nhà cung cấp", [Suppliers.View, Suppliers.Create, Suppliers.Edit, Suppliers.Delete] },
        { "Nhập hàng", [Inputs.View, Inputs.Create, Inputs.Edit, Inputs.Delete, Inputs.ChangeStatus] },
        { "Xuất hàng", [Outputs.View, Outputs.Create, Outputs.Edit, Outputs.Delete, Outputs.ChangeStatus] },
        { "Tệp tin", [Files.View, Files.Upload, Files.Delete] },
        { "Cài đặt", [Settings.View, Settings.Edit] },
        { "Thống kê", [Statistical.View, Statistical.Export] },
        { "Vai trò", [Roles.View, Roles.Create, Roles.Edit, Roles.Delete] },
        { "Người dùng", [Users.View, Users.Create, Users.Edit, Users.Delete, Users.AssignRoles, Users.ChangePassword] },
        { "Tin tức", [News.View, News.Create, News.Edit, News.Delete] },
        { "Banner", [Banners.View, Banners.Create, Banners.Edit, Banners.Delete] },
        {
            "CRM & Connect",
            [Contacts.View, Contacts.Reply, Contacts.EditNote, Contacts.Delete, Bookings.View, Bookings.Confirm, Bookings.Delete, Leads.View, Leads.Create, Leads.Edit, Leads.Delete]
        },
    };

    public static readonly Dictionary<string, List<string>> Conflicts = new() { };

    public static readonly Dictionary<string, List<string>> Dependencies = new()
    {
        { Brands.Create, [Brands.View] },
        { Brands.Edit, [Brands.View] },
        { Brands.Delete, [Brands.View] },

        { Products.Create, [Products.View] },
        { Products.Edit, [Products.View] },
        { Products.Delete, [Products.View] },
        { Products.EditPrice, [Products.View] },
        { Products.ChangeStatus, [Products.View] },

        { ProductCategories.Create, [ProductCategories.View] },
        { ProductCategories.Edit, [ProductCategories.View] },
        { ProductCategories.Delete, [ProductCategories.View] },

        { Suppliers.Create, [Suppliers.View] },
        { Suppliers.Edit, [Suppliers.View] },
        { Suppliers.Delete, [Suppliers.View] },

        { Inputs.Create, [Inputs.View] },
        { Inputs.Edit, [Inputs.View] },
        { Inputs.Delete, [Inputs.View] },
        { Inputs.ChangeStatus, [Inputs.View] },

        { Outputs.Create, [Outputs.View] },
        { Outputs.Edit, [Outputs.View] },
        { Outputs.Delete, [Outputs.View] },
        { Outputs.ChangeStatus, [Outputs.View] },

        { Files.Upload, [Files.View] },
        { Files.Delete, [Files.View] },

        { Settings.Edit, [Settings.View] },

        { Statistical.Export, [Statistical.View] },

        { Roles.Create, [Roles.View] },
        { Roles.Edit, [Roles.View] },
        { Roles.Delete, [Roles.View] },

        { Users.Create, [Users.View] },
        { Users.Edit, [Users.View] },
        { Users.Delete, [Users.View] },
        { Users.AssignRoles, [Users.View, Roles.View] },
        { Users.ChangePassword, [Users.View] },

        { News.Create, [News.View] },
        { News.Edit, [News.View] },
        { News.Delete, [News.View] },

        { Banners.Create, [Banners.View] },
        { Banners.Edit, [Banners.View] },
        { Banners.Delete, [Banners.View] },

        { Contacts.Reply, [Contacts.View] },
        { Contacts.EditNote, [Contacts.View] },
        { Contacts.Delete, [Contacts.View] },

        { Bookings.Confirm, [Bookings.View] },
        { Bookings.Delete, [Bookings.View] },

        { Leads.Create, [Leads.View] },
        { Leads.Edit, [Leads.View] },
        { Leads.Delete, [Leads.View] },
    };

    public static (bool IsValid, string? ErrorMessage) ValidateRules(IEnumerable<string> permissions)
    {
        if (permissions == null)
            return (true, null);
        var permSet = permissions.ToHashSet();
        foreach (var perm in permSet)
        {
            if (Conflicts.TryGetValue(perm, out var conflicts))
            {
                var intersected = conflicts.Intersect(permSet).ToList();
                if (intersected.Count != 0)
                {
                    return (false, $"Permission '{perm}' conflicts with: {string.Join(", ", intersected)}");
                }
            }
            if (Dependencies.TryGetValue(perm, out var dependencies))
            {
                var missing = dependencies.Where(d => !permSet.Contains(d)).ToList();
                if (missing.Count != 0)
                {
                    return (false, $"Permission '{perm}' requires: {string.Join(", ", missing)}");
                }
            }
        }
        return (true, null);
    }
}
