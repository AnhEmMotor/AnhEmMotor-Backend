using Permissions = Domain.Constants.Permission.Permissions;

namespace Domain.Constants.Permission;

public static class PermissionsList
{

    private static readonly Dictionary<string, PermissionMetadata> PermissionMetadataMap = new()
    {
        { Permissions.Brands.View, new PermissionMetadata("View Brands", "Xem danh sách thương hiệu") },
        { Permissions.Brands.Create, new PermissionMetadata("Create Brand", "Tạo thương hiệu mới") },
        { Permissions.Brands.Edit, new PermissionMetadata("Edit Brand", "Chỉnh sửa thương hiệu") },
        { Permissions.Brands.Delete, new PermissionMetadata("Delete Brand", "Xóa thương hiệu") },

        { Permissions.Products.View, new PermissionMetadata("View Products", "Xem danh sách sản phẩm") },
        { Permissions.Products.Create, new PermissionMetadata("Create Product", "Tạo sản phẩm mới") },
        { Permissions.Products.Edit, new PermissionMetadata("Edit Product", "Chỉnh sửa sản phẩm") },
        { Permissions.Products.Delete, new PermissionMetadata("Delete Product", "Xóa sản phẩm") },
        { Permissions.Products.EditPrice, new PermissionMetadata("Edit Price", "Chỉnh giá của sản phẩm") },
        {
            Permissions.Products.ChangeStatus,
            new PermissionMetadata("Change Status", "Chỉnh trạng thái của sản phẩm (đang bán hoặc ngưng bán)")
        },

        { Permissions.ProductCategories.View, new PermissionMetadata("View Product Categories", "Xem danh mục sản phẩm") },
        { Permissions.ProductCategories.Create, new PermissionMetadata("Create Product Category", "Tạo danh mục sản phẩm mới") },
        { Permissions.ProductCategories.Edit, new PermissionMetadata("Edit Product Category", "Chỉnh sửa danh mục sản phẩm") },
        { Permissions.ProductCategories.Delete, new PermissionMetadata("Delete Product Category", "Xóa danh mục sản phẩm") },

        { Permissions.Suppliers.View, new PermissionMetadata("View Suppliers", "Xem danh sách nhà cung cấp") },
        { Permissions.Suppliers.Create, new PermissionMetadata("Create Supplier", "Tạo nhà cung cấp mới") },
        { Permissions.Suppliers.Edit, new PermissionMetadata("Edit Supplier", "Chỉnh sửa nhà cung cấp") },
        { Permissions.Suppliers.Delete, new PermissionMetadata("Delete Supplier", "Xóa nhà cung cấp") },

        { Permissions.Inputs.View, new PermissionMetadata("View Inputs", "Xem danh sách đơn nhập hàng") },
        { Permissions.Inputs.Create, new PermissionMetadata("Create Input", "Tạo đơn nhập hàng mới") },
        { Permissions.Inputs.Edit, new PermissionMetadata("Edit Input", "Chỉnh sửa đơn nhập hàng") },
        { Permissions.Inputs.Delete, new PermissionMetadata("Delete Input", "Xóa đơn nhập hàng") },
        {
            Permissions.Inputs.ChangeStatus,
            new PermissionMetadata("Change Status", "Thay đổi trạng thái đơn nhập hàng (hoàn tất hoặc huỷ bỏ)")
        },

        { Permissions.Outputs.View, new PermissionMetadata("View Outputs", "Xem danh sách đơn xuất hàng") },
        { Permissions.Outputs.Create, new PermissionMetadata("Create Output", "Tạo đơn xuất hàng mới") },
        { Permissions.Outputs.Edit, new PermissionMetadata("Edit Output", "Chỉnh sửa đơn xuất hàng") },
        { Permissions.Outputs.Delete, new PermissionMetadata("Delete Output", "Xóa đơn xuất hàng") },
        { Permissions.Outputs.ChangeStatus, new PermissionMetadata("Change Status", "Thay đổi trạng thái đơn hàng") },

        { Permissions.Files.View, new PermissionMetadata("View Files", "Xem danh sách tệp tin") },
        { Permissions.Files.Upload, new PermissionMetadata("Upload File", "Tải lên tệp tin") },
        { Permissions.Files.Delete, new PermissionMetadata("Delete File", "Xóa tệp tin") },

        { Permissions.Settings.View, new PermissionMetadata("View Settings", "Xem cài đặt hệ thống") },
        { Permissions.Settings.Edit, new PermissionMetadata("Edit Settings", "Chỉnh sửa cài đặt hệ thống") },

        { Permissions.Statistical.View, new PermissionMetadata("View Statistics", "Xem thống kê") },
        { Permissions.Statistical.Export, new PermissionMetadata("Export Statistics", "Xuất báo cáo thống kê") },

        { Permissions.Roles.View, new PermissionMetadata("View Roles", "Xem danh sách vai trò") },
        { Permissions.Roles.Create, new PermissionMetadata("Create Role", "Tạo vai trò mới") },
        { Permissions.Roles.Edit, new PermissionMetadata("Edit Role", "Chỉnh sửa vai trò") },
        { Permissions.Roles.Delete, new PermissionMetadata("Delete Role", "Xóa vai trò") },

        { Permissions.Users.View, new PermissionMetadata("View Users", "Xem danh sách người dùng") },
        { Permissions.Users.Create, new PermissionMetadata("Create User", "Tạo người dùng mới") },
        { Permissions.Users.Edit, new PermissionMetadata("Edit User", "Chỉnh sửa thông tin người dùng") },
        { Permissions.Users.Delete, new PermissionMetadata("Delete User", "Xóa người dùng") },
        { Permissions.Users.AssignRoles, new PermissionMetadata("Assign Roles", "Gán vai trò cho người dùng") },
        { Permissions.Users.ChangePassword, new PermissionMetadata("Change Password", "Đổi mật khẩu người dùng") },

        { Permissions.News.View, new PermissionMetadata("View News", "Xem danh sách tin tức") },
        { Permissions.News.Create, new PermissionMetadata("Create News", "Tạo tin tức mới") },
        { Permissions.News.Edit, new PermissionMetadata("Edit News", "Chỉnh sửa tin tức") },
        { Permissions.News.Delete, new PermissionMetadata("Delete News", "Xóa tin tức") },

        { Permissions.Banners.View, new PermissionMetadata("View Banners", "Xem danh sách banner") },
        { Permissions.Banners.Create, new PermissionMetadata("Create Banner", "Thêm banner mới") },
        { Permissions.Banners.Edit, new PermissionMetadata("Edit Banner", "Chỉnh sửa banner") },
        { Permissions.Banners.Delete, new PermissionMetadata("Delete Banner", "Xóa banner") },

        { Permissions.Contacts.View, new PermissionMetadata("View Contacts", "Xem danh sách liên hệ khách hàng") },
        { Permissions.Contacts.Reply, new PermissionMetadata("Reply to Contact", "Phản hồi liên hệ khách hàng") },
        { Permissions.Contacts.EditNote, new PermissionMetadata("Edit Internal Note", "Chỉnh sửa ghi chú nội bộ khách hàng") },
        { Permissions.Contacts.Delete, new PermissionMetadata("Delete Contact", "Xóa liên hệ khách hàng") },

        { Permissions.Bookings.View, new PermissionMetadata("View Bookings", "Xem danh sách đặt lịch lái thử") },
        { Permissions.Bookings.Confirm, new PermissionMetadata("Confirm Booking", "Xác nhận lịch hẹn lái thử") },
        { Permissions.Bookings.Delete, new PermissionMetadata("Delete Booking", "Xóa lịch hẹn lái thử") },

        { Permissions.Leads.View, new PermissionMetadata("View Leads", "Xem danh sách khách hàng tiềm năng") },
        { Permissions.Leads.Create, new PermissionMetadata("Create Lead", "Tạo khách hàng tiềm năng") },
        { Permissions.Leads.Edit, new PermissionMetadata("Edit Lead", "Chỉnh sửa khách hàng tiềm năng") },
        { Permissions.Leads.Delete, new PermissionMetadata("Delete Lead", "Xóa khách hàng tiềm năng") },

        { Permissions.HR.View, new PermissionMetadata("View HR", "Xem danh sách nhân sự") },
        { Permissions.HR.Create, new PermissionMetadata("Create Employee", "Thêm nhân viên mới") },
        { Permissions.HR.Edit, new PermissionMetadata("Edit Employee", "Chỉnh sửa thông tin nhân viên") },
        { Permissions.HR.Delete, new PermissionMetadata("Delete Employee", "Xóa nhân viên") },

        { Permissions.Payroll.View, new PermissionMetadata("View Payroll", "Xem Bảng Lương") },
        { Permissions.Payroll.Configure, new PermissionMetadata("Configure Payroll", "Cấu hình chính sách hoa hồng") },
        { Permissions.Payroll.Approve, new PermissionMetadata("Approve Payroll", "Duyệt chi lương") },
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
        { "Thương hiệu", [Permissions.Brands.View, Permissions.Brands.Create, Permissions.Brands.Edit, Permissions.Brands.Delete] },
        {
            "Sản phẩm",
            [Permissions.Products.View, Permissions.Products.Create, Permissions.Products.Edit, Permissions.Products.Delete, Permissions.Products.EditPrice, Permissions.Products.ChangeStatus]
        },
        {
            "Danh mục sản phẩm",
            [Permissions.ProductCategories.View, Permissions.ProductCategories.Create, Permissions.ProductCategories.Edit, Permissions.ProductCategories.Delete]
        },
        { "Nhà cung cấp", [Permissions.Suppliers.View, Permissions.Suppliers.Create, Permissions.Suppliers.Edit, Permissions.Suppliers.Delete] },
        { "Nhập hàng", [Permissions.Inputs.View, Permissions.Inputs.Create, Permissions.Inputs.Edit, Permissions.Inputs.Delete, Permissions.Inputs.ChangeStatus] },
        { "Xuất hàng", [Permissions.Outputs.View, Permissions.Outputs.Create, Permissions.Outputs.Edit, Permissions.Outputs.Delete, Permissions.Outputs.ChangeStatus] },
        { "Tệp tin", [Permissions.Files.View, Permissions.Files.Upload, Permissions.Files.Delete] },
        { "Cài đặt", [Permissions.Settings.View, Permissions.Settings.Edit] },
        { "Thống kê", [Permissions.Statistical.View, Permissions.Statistical.Export] },
        { "Vai trò", [Permissions.Roles.View, Permissions.Roles.Create, Permissions.Roles.Edit, Permissions.Roles.Delete] },
        { "Người dùng", [Permissions.Users.View, Permissions.Users.Create, Permissions.Users.Edit, Permissions.Users.Delete, Permissions.Users.AssignRoles, Permissions.Users.ChangePassword] },
        { "Tin tức", [Permissions.News.View, Permissions.News.Create, Permissions.News.Edit, Permissions.News.Delete] },
        { "Banner", [Permissions.Banners.View, Permissions.Banners.Create, Permissions.Banners.Edit, Permissions.Banners.Delete] },
        {
            "CRM & Connect",
            [Permissions.Contacts.View, Permissions.Contacts.Reply, Permissions.Contacts.EditNote, Permissions.Contacts.Delete, Permissions.Bookings.View, Permissions.Bookings.Confirm, Permissions.Bookings.Delete, Permissions.Leads.View, Permissions.Leads.Create, Permissions.Leads.Edit, Permissions.Leads.Delete]
        },
        { "Nhân sự", [Permissions.HR.View, Permissions.HR.Create, Permissions.HR.Edit, Permissions.HR.Delete] },
        { "Lương & Hoa hồng", [Permissions.Payroll.View, Permissions.Payroll.Configure, Permissions.Payroll.Approve] },
    };

    public static readonly Dictionary<string, List<string>> Conflicts = new() { };

    public static readonly Dictionary<string, List<string>> Dependencies = new()
    {
        { Permissions.Brands.Create, [Permissions.Brands.View] },
        { Permissions.Brands.Edit, [Permissions.Brands.View] },
        { Permissions.Brands.Delete, [Permissions.Brands.View] },

        { Permissions.Products.Create, [Permissions.Products.View] },
        { Permissions.Products.Edit, [Permissions.Products.View] },
        { Permissions.Products.Delete, [Permissions.Products.View] },
        { Permissions.Products.EditPrice, [Permissions.Products.View] },
        { Permissions.Products.ChangeStatus, [Permissions.Products.View] },

        { Permissions.ProductCategories.Create, [Permissions.ProductCategories.View] },
        { Permissions.ProductCategories.Edit, [Permissions.ProductCategories.View] },
        { Permissions.ProductCategories.Delete, [Permissions.ProductCategories.View] },

        { Permissions.Suppliers.Create, [Permissions.Suppliers.View] },
        { Permissions.Suppliers.Edit, [Permissions.Suppliers.View] },
        { Permissions.Suppliers.Delete, [Permissions.Suppliers.View] },

        { Permissions.Inputs.Create, [Permissions.Inputs.View] },
        { Permissions.Inputs.Edit, [Permissions.Inputs.View] },
        { Permissions.Inputs.Delete, [Permissions.Inputs.View] },
        { Permissions.Inputs.ChangeStatus, [Permissions.Inputs.View] },

        { Permissions.Outputs.Create, [Permissions.Outputs.View] },
        { Permissions.Outputs.Edit, [Permissions.Outputs.View] },
        { Permissions.Outputs.Delete, [Permissions.Outputs.View] },
        { Permissions.Outputs.ChangeStatus, [Permissions.Outputs.View] },

        { Permissions.Files.Upload, [Permissions.Files.View] },
        { Permissions.Files.Delete, [Permissions.Files.View] },

        { Permissions.Settings.Edit, [Permissions.Settings.View] },

        { Permissions.Statistical.Export, [Permissions.Statistical.View] },

        { Permissions.Roles.Create, [Permissions.Roles.View] },
        { Permissions.Roles.Edit, [Permissions.Roles.View] },
        { Permissions.Roles.Delete, [Permissions.Roles.View] },

        { Permissions.Users.Create, [Permissions.Users.View] },
        { Permissions.Users.Edit, [Permissions.Users.View] },
        { Permissions.Users.Delete, [Permissions.Users.View] },
        { Permissions.Users.AssignRoles, [Permissions.Users.View, Permissions.Roles.View] },
        { Permissions.Users.ChangePassword, [Permissions.Users.View] },

        { Permissions.News.Create, [Permissions.News.View] },
        { Permissions.News.Edit, [Permissions.News.View] },
        { Permissions.News.Delete, [Permissions.News.View] },

        { Permissions.Banners.Create, [Permissions.Banners.View] },
        { Permissions.Banners.Edit, [Permissions.Banners.View] },
        { Permissions.Banners.Delete, [Permissions.Banners.View] },

        { Permissions.Contacts.Reply, [Permissions.Contacts.View] },
        { Permissions.Contacts.EditNote, [Permissions.Contacts.View] },
        { Permissions.Contacts.Delete, [Permissions.Contacts.View] },

        { Permissions.Bookings.Confirm, [Permissions.Bookings.View] },
        { Permissions.Bookings.Delete, [Permissions.Bookings.View] },

        { Permissions.Leads.Create, [Permissions.Leads.View] },
        { Permissions.Leads.Edit, [Permissions.Leads.View] },
        { Permissions.Leads.Delete, [Permissions.Leads.View] },

        { Permissions.HR.Create, [Permissions.HR.View] },
        { Permissions.HR.Edit, [Permissions.HR.View] },
        { Permissions.HR.Delete, [Permissions.HR.View] },

        { Permissions.Payroll.Configure, [Permissions.Payroll.View] },
        { Permissions.Payroll.Approve, [Permissions.Payroll.View] },
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

