using System.Collections.Generic;
using System.Linq;
using Domain.Constants.Permission;

namespace Domain.Constants.Permission;

public static class PermissionsList
{
    public static readonly List<PermissionModuleMetadata> ModulesTree =
    [
        new PermissionModuleMetadata
        {
            Id = "Permissions.Admin",
            Name = "Ban Điều Hành & Chủ Showroom",
            Features =
            [
                new PermissionFeatureMetadata { Id = "Permissions.Admin.EmployeeManagement", Name = "Quản lý nhân viên", Permissions = new() {
                    new PermissionActionMetadata { Id = Permissions.Admin.EmployeeManagement.View, Name = "Xem", Description = "Xem quản lý nhân viên" },
                    new PermissionActionMetadata { Id = Permissions.Admin.EmployeeManagement.Create, Name = "Tạo mới", Description = "Tạo mới quản lý nhân viên" },
                    new PermissionActionMetadata { Id = Permissions.Admin.EmployeeManagement.Edit, Name = "Chỉnh sửa", Description = "Chỉnh sửa quản lý nhân viên" },
                    new PermissionActionMetadata { Id = Permissions.Admin.EmployeeManagement.Delete, Name = "Xóa", Description = "Xóa quản lý nhân viên" },
                }},
                new PermissionFeatureMetadata { Id = "Permissions.Admin.PayrollManagement", Name = "Quản lý lương", Permissions = new() {
                    new PermissionActionMetadata { Id = Permissions.Admin.PayrollManagement.View, Name = "Xem", Description = "Xem quản lý lương" },
                    new PermissionActionMetadata { Id = Permissions.Admin.PayrollManagement.Configure, Name = "Cấu hình", Description = "Cấu hình quản lý lương" },
                    new PermissionActionMetadata { Id = Permissions.Admin.PayrollManagement.Approve, Name = "Phê duyệt", Description = "Phê duyệt quản lý lương" },
                }},
                new PermissionFeatureMetadata { Id = "Permissions.Admin.DashboardManagement", Name = "Tổng quan (Dashboard)", Permissions = new() {
                    new PermissionActionMetadata { Id = Permissions.Admin.DashboardManagement.View, Name = "Xem", Description = "Xem tổng quan (dashboard)" },
                    new PermissionActionMetadata { Id = Permissions.Admin.DashboardManagement.Export, Name = "Xuất dữ liệu", Description = "Xuất dữ liệu tổng quan (dashboard)" },
                }},
                new PermissionFeatureMetadata { Id = "Permissions.Admin.ContractManagement", Name = "Quản lý hợp đồng", Permissions = new() {
                    new PermissionActionMetadata { Id = Permissions.Admin.ContractManagement.View, Name = "Xem", Description = "Xem quản lý hợp đồng" },
                    new PermissionActionMetadata { Id = Permissions.Admin.ContractManagement.Create, Name = "Tạo mới", Description = "Tạo mới quản lý hợp đồng" },
                    new PermissionActionMetadata { Id = Permissions.Admin.ContractManagement.Edit, Name = "Chỉnh sửa", Description = "Chỉnh sửa quản lý hợp đồng" },
                    new PermissionActionMetadata { Id = Permissions.Admin.ContractManagement.Delete, Name = "Xóa", Description = "Xóa quản lý hợp đồng" },
                }},
                new PermissionFeatureMetadata { Id = "Permissions.Admin.FileManagement", Name = "Quản lý tệp", Permissions = new() {
                    new PermissionActionMetadata { Id = Permissions.Admin.FileManagement.View, Name = "Xem", Description = "Xem quản lý tệp" },
                    new PermissionActionMetadata { Id = Permissions.Admin.FileManagement.Upload, Name = "Tải lên", Description = "Tải lên quản lý tệp" },
                    new PermissionActionMetadata { Id = Permissions.Admin.FileManagement.Delete, Name = "Xóa", Description = "Xóa quản lý tệp" },
                }},
                new PermissionFeatureMetadata { Id = "Permissions.Admin.RoleManagement", Name = "Quản lý phân quyền", Permissions = new() {
                    new PermissionActionMetadata { Id = Permissions.Admin.RoleManagement.View, Name = "Xem", Description = "Xem quản lý phân quyền" },
                    new PermissionActionMetadata { Id = Permissions.Admin.RoleManagement.Create, Name = "Tạo mới", Description = "Tạo mới quản lý phân quyền" },
                    new PermissionActionMetadata { Id = Permissions.Admin.RoleManagement.Edit, Name = "Chỉnh sửa", Description = "Chỉnh sửa quản lý phân quyền" },
                    new PermissionActionMetadata { Id = Permissions.Admin.RoleManagement.Delete, Name = "Xóa", Description = "Xóa quản lý phân quyền" },
                }},
                new PermissionFeatureMetadata { Id = "Permissions.Admin.SettingManagement", Name = "Cài đặt hệ thống", Permissions = new() {
                    new PermissionActionMetadata { Id = Permissions.Admin.SettingManagement.View, Name = "Xem", Description = "Xem cài đặt hệ thống" },
                    new PermissionActionMetadata { Id = Permissions.Admin.SettingManagement.Edit, Name = "Chỉnh sửa", Description = "Chỉnh sửa cài đặt hệ thống" },
                }},
                new PermissionFeatureMetadata { Id = "Permissions.Admin.UserManagement", Name = "Quản lý người dùng", Permissions = new() {
                    new PermissionActionMetadata { Id = Permissions.Admin.UserManagement.View, Name = "Xem", Description = "Xem quản lý người dùng" },
                    new PermissionActionMetadata { Id = Permissions.Admin.UserManagement.Create, Name = "Tạo mới", Description = "Tạo mới quản lý người dùng" },
                    new PermissionActionMetadata { Id = Permissions.Admin.UserManagement.Edit, Name = "Chỉnh sửa", Description = "Chỉnh sửa quản lý người dùng" },
                    new PermissionActionMetadata { Id = Permissions.Admin.UserManagement.Delete, Name = "Xóa", Description = "Xóa quản lý người dùng" },
                    new PermissionActionMetadata { Id = Permissions.Admin.UserManagement.AssignRoles, Name = "Gán quyền", Description = "Gán quyền quản lý người dùng" },
                    new PermissionActionMetadata { Id = Permissions.Admin.UserManagement.ChangePassword, Name = "Đổi mật khẩu", Description = "Đổi mật khẩu quản lý người dùng" },
                }}
            ]
        },
        new PermissionModuleMetadata
        {
            Id = "Permissions.Marketing",
            Name = "Marketing & SEO",
            Features =
            [
                new PermissionFeatureMetadata { Id = "Permissions.Marketing.BannerManagement", Name = "Quản lý Banner", Permissions = new() {
                    new PermissionActionMetadata { Id = Permissions.Marketing.BannerManagement.View, Name = "Xem", Description = "Xem quản lý banner" },
                    new PermissionActionMetadata { Id = Permissions.Marketing.BannerManagement.Create, Name = "Tạo mới", Description = "Tạo mới quản lý banner" },
                    new PermissionActionMetadata { Id = Permissions.Marketing.BannerManagement.Edit, Name = "Chỉnh sửa", Description = "Chỉnh sửa quản lý banner" },
                    new PermissionActionMetadata { Id = Permissions.Marketing.BannerManagement.Delete, Name = "Xóa", Description = "Xóa quản lý banner" },
                }},
                new PermissionFeatureMetadata { Id = "Permissions.Marketing.CustomerManagement", Name = "Quản lý khách hàng", Permissions = new() {
                    new PermissionActionMetadata { Id = Permissions.Marketing.CustomerManagement.View, Name = "Xem", Description = "Xem quản lý khách hàng" },
                    new PermissionActionMetadata { Id = Permissions.Marketing.CustomerManagement.Reply, Name = "Phản hồi", Description = "Phản hồi quản lý khách hàng" },
                    new PermissionActionMetadata { Id = Permissions.Marketing.CustomerManagement.EditNote, Name = "Sửa ghi chú", Description = "Sửa ghi chú quản lý khách hàng" },
                    new PermissionActionMetadata { Id = Permissions.Marketing.CustomerManagement.Delete, Name = "Xóa", Description = "Xóa quản lý khách hàng" },
                }},
                new PermissionFeatureMetadata { Id = "Permissions.Marketing.LeadManagement", Name = "Quản lý KH tiềm năng", Permissions = new() {
                    new PermissionActionMetadata { Id = Permissions.Marketing.LeadManagement.View, Name = "Xem", Description = "Xem quản lý kh tiềm năng" },
                    new PermissionActionMetadata { Id = Permissions.Marketing.LeadManagement.Create, Name = "Tạo mới", Description = "Tạo mới quản lý kh tiềm năng" },
                    new PermissionActionMetadata { Id = Permissions.Marketing.LeadManagement.Edit, Name = "Chỉnh sửa", Description = "Chỉnh sửa quản lý kh tiềm năng" },
                    new PermissionActionMetadata { Id = Permissions.Marketing.LeadManagement.Delete, Name = "Xóa", Description = "Xóa quản lý kh tiềm năng" },
                }},
                new PermissionFeatureMetadata { Id = "Permissions.Marketing.NewsManagement", Name = "Quản lý tin tức", Permissions = new() {
                    new PermissionActionMetadata { Id = Permissions.Marketing.NewsManagement.View, Name = "Xem", Description = "Xem quản lý tin tức" },
                    new PermissionActionMetadata { Id = Permissions.Marketing.NewsManagement.Create, Name = "Tạo mới", Description = "Tạo mới quản lý tin tức" },
                    new PermissionActionMetadata { Id = Permissions.Marketing.NewsManagement.Edit, Name = "Chỉnh sửa", Description = "Chỉnh sửa quản lý tin tức" },
                    new PermissionActionMetadata { Id = Permissions.Marketing.NewsManagement.Delete, Name = "Xóa", Description = "Xóa quản lý tin tức" },
                }},
                new PermissionFeatureMetadata { Id = "Permissions.Marketing.BookingManagement", Name = "Quản lý đặt lịch (Booking)", Permissions = new() {
                    new PermissionActionMetadata { Id = Permissions.Marketing.BookingManagement.View, Name = "Xem", Description = "Xem quản lý đặt lịch (booking)" },
                    new PermissionActionMetadata { Id = Permissions.Marketing.BookingManagement.Confirm, Name = "Xác nhận", Description = "Xác nhận quản lý đặt lịch (booking)" },
                    new PermissionActionMetadata { Id = Permissions.Marketing.BookingManagement.Delete, Name = "Xóa", Description = "Xóa quản lý đặt lịch (booking)" },
                }}
            ]
        },
        new PermissionModuleMetadata
        {
            Id = "Permissions.Warehouse",
            Name = "Quản lý Kho & Hậu Cần",
            Features =
            [
                new PermissionFeatureMetadata { Id = "Permissions.Warehouse.DebtPaymentManagement", Name = "Quản lý công nợ", Permissions = new() {
                    new PermissionActionMetadata { Id = Permissions.Warehouse.DebtPaymentManagement.View, Name = "Xem", Description = "Xem quản lý công nợ" },
                    new PermissionActionMetadata { Id = Permissions.Warehouse.DebtPaymentManagement.Create, Name = "Tạo mới", Description = "Tạo mới quản lý công nợ" },
                    new PermissionActionMetadata { Id = Permissions.Warehouse.DebtPaymentManagement.Edit, Name = "Chỉnh sửa", Description = "Chỉnh sửa quản lý công nợ" },
                }},
                new PermissionFeatureMetadata { Id = "Permissions.Warehouse.BrandManagement", Name = "Quản lý thương hiệu", Permissions = new() {
                    new PermissionActionMetadata { Id = Permissions.Warehouse.BrandManagement.View, Name = "Xem", Description = "Xem quản lý thương hiệu" },
                    new PermissionActionMetadata { Id = Permissions.Warehouse.BrandManagement.Create, Name = "Tạo mới", Description = "Tạo mới quản lý thương hiệu" },
                    new PermissionActionMetadata { Id = Permissions.Warehouse.BrandManagement.Edit, Name = "Chỉnh sửa", Description = "Chỉnh sửa quản lý thương hiệu" },
                    new PermissionActionMetadata { Id = Permissions.Warehouse.BrandManagement.Delete, Name = "Xóa", Description = "Xóa quản lý thương hiệu" },
                }},
                new PermissionFeatureMetadata { Id = "Permissions.Warehouse.CategoryManagement", Name = "Quản lý danh mục", Permissions = new() {
                    new PermissionActionMetadata { Id = Permissions.Warehouse.CategoryManagement.View, Name = "Xem", Description = "Xem quản lý danh mục" },
                    new PermissionActionMetadata { Id = Permissions.Warehouse.CategoryManagement.Create, Name = "Tạo mới", Description = "Tạo mới quản lý danh mục" },
                    new PermissionActionMetadata { Id = Permissions.Warehouse.CategoryManagement.Edit, Name = "Chỉnh sửa", Description = "Chỉnh sửa quản lý danh mục" },
                    new PermissionActionMetadata { Id = Permissions.Warehouse.CategoryManagement.Delete, Name = "Xóa", Description = "Xóa quản lý danh mục" },
                }},
                new PermissionFeatureMetadata { Id = "Permissions.Warehouse.OutputManagement", Name = "Quản lý phiếu xuất", Permissions = new() {
                    new PermissionActionMetadata { Id = Permissions.Warehouse.OutputManagement.View, Name = "Xem", Description = "Xem quản lý phiếu xuất" },
                    new PermissionActionMetadata { Id = Permissions.Warehouse.OutputManagement.ViewConfirmed, Name = "Xem (đã duyệt)", Description = "Xem (đã duyệt) quản lý phiếu xuất" },
                    new PermissionActionMetadata { Id = Permissions.Warehouse.OutputManagement.ViewUnconfirmed, Name = "Xem (chưa duyệt)", Description = "Xem (chưa duyệt) quản lý phiếu xuất" },
                    new PermissionActionMetadata { Id = Permissions.Warehouse.OutputManagement.Create, Name = "Tạo mới", Description = "Tạo mới quản lý phiếu xuất" },
                    new PermissionActionMetadata { Id = Permissions.Warehouse.OutputManagement.Edit, Name = "Chỉnh sửa", Description = "Chỉnh sửa quản lý phiếu xuất" },
                    new PermissionActionMetadata { Id = Permissions.Warehouse.OutputManagement.Delete, Name = "Xóa", Description = "Xóa quản lý phiếu xuất" },
                    new PermissionActionMetadata { Id = Permissions.Warehouse.OutputManagement.ChangeStatus, Name = "Đổi trạng thái", Description = "Đổi trạng thái quản lý phiếu xuất" },
                }},
                new PermissionFeatureMetadata { Id = "Permissions.Warehouse.ProductManagement", Name = "Quản lý sản phẩm", Permissions = new() {
                    new PermissionActionMetadata { Id = Permissions.Warehouse.ProductManagement.View, Name = "Xem", Description = "Xem quản lý sản phẩm" },
                    new PermissionActionMetadata { Id = Permissions.Warehouse.ProductManagement.Create, Name = "Tạo mới", Description = "Tạo mới quản lý sản phẩm" },
                    new PermissionActionMetadata { Id = Permissions.Warehouse.ProductManagement.Edit, Name = "Chỉnh sửa", Description = "Chỉnh sửa quản lý sản phẩm" },
                    new PermissionActionMetadata { Id = Permissions.Warehouse.ProductManagement.Delete, Name = "Xóa", Description = "Xóa quản lý sản phẩm" },
                    new PermissionActionMetadata { Id = Permissions.Warehouse.ProductManagement.EditPrice, Name = "Chỉnh giá", Description = "Chỉnh giá quản lý sản phẩm" },
                    new PermissionActionMetadata { Id = Permissions.Warehouse.ProductManagement.ChangeStatus, Name = "Đổi trạng thái", Description = "Đổi trạng thái quản lý sản phẩm" },
                }},
                new PermissionFeatureMetadata { Id = "Permissions.Warehouse.PurchaseOrderManagement", Name = "Quản lý phiếu mua hàng (PO)", Permissions = new() {
                    new PermissionActionMetadata { Id = Permissions.Warehouse.PurchaseOrderManagement.View, Name = "Xem", Description = "Xem quản lý phiếu mua hàng (po)" },
                    new PermissionActionMetadata { Id = Permissions.Warehouse.PurchaseOrderManagement.Create, Name = "Tạo mới", Description = "Tạo mới quản lý phiếu mua hàng (po)" },
                    new PermissionActionMetadata { Id = Permissions.Warehouse.PurchaseOrderManagement.Edit, Name = "Chỉnh sửa", Description = "Chỉnh sửa quản lý phiếu mua hàng (po)" },
                    new PermissionActionMetadata { Id = Permissions.Warehouse.PurchaseOrderManagement.Delete, Name = "Xóa", Description = "Xóa quản lý phiếu mua hàng (po)" },
                    new PermissionActionMetadata { Id = Permissions.Warehouse.PurchaseOrderManagement.Send, Name = "Gửi", Description = "Gửi quản lý phiếu mua hàng (po)" },
                    new PermissionActionMetadata { Id = Permissions.Warehouse.PurchaseOrderManagement.ApproveReject, Name = "Duyệt/Từ chối", Description = "Duyệt/Từ chối quản lý phiếu mua hàng (po)" },
                }},
                new PermissionFeatureMetadata { Id = "Permissions.Warehouse.PurchaseRequestManagement", Name = "Quản lý yêu cầu mua hàng (PR)", Permissions = new() {
                    new PermissionActionMetadata { Id = Permissions.Warehouse.PurchaseRequestManagement.View, Name = "Xem", Description = "Xem quản lý yêu cầu mua hàng (pr)" },
                    new PermissionActionMetadata { Id = Permissions.Warehouse.PurchaseRequestManagement.Create, Name = "Tạo mới", Description = "Tạo mới quản lý yêu cầu mua hàng (pr)" },
                    new PermissionActionMetadata { Id = Permissions.Warehouse.PurchaseRequestManagement.Edit, Name = "Chỉnh sửa", Description = "Chỉnh sửa quản lý yêu cầu mua hàng (pr)" },
                    new PermissionActionMetadata { Id = Permissions.Warehouse.PurchaseRequestManagement.Delete, Name = "Xóa", Description = "Xóa quản lý yêu cầu mua hàng (pr)" },
                    new PermissionActionMetadata { Id = Permissions.Warehouse.PurchaseRequestManagement.Send, Name = "Gửi", Description = "Gửi quản lý yêu cầu mua hàng (pr)" },
                    new PermissionActionMetadata { Id = Permissions.Warehouse.PurchaseRequestManagement.ApproveReject, Name = "Duyệt/Từ chối", Description = "Duyệt/Từ chối quản lý yêu cầu mua hàng (pr)" },
                }},
                new PermissionFeatureMetadata { Id = "Permissions.Warehouse.ReceiptManagement", Name = "Quản lý phiếu nhập kho", Permissions = new() {
                    new PermissionActionMetadata { Id = Permissions.Warehouse.ReceiptManagement.View, Name = "Xem", Description = "Xem quản lý phiếu nhập kho" },
                    new PermissionActionMetadata { Id = Permissions.Warehouse.ReceiptManagement.Create, Name = "Tạo mới", Description = "Tạo mới quản lý phiếu nhập kho" },
                    new PermissionActionMetadata { Id = Permissions.Warehouse.ReceiptManagement.Edit, Name = "Chỉnh sửa", Description = "Chỉnh sửa quản lý phiếu nhập kho" },
                    new PermissionActionMetadata { Id = Permissions.Warehouse.ReceiptManagement.Delete, Name = "Xóa", Description = "Xóa quản lý phiếu nhập kho" },
                    new PermissionActionMetadata { Id = Permissions.Warehouse.ReceiptManagement.Send, Name = "Gửi", Description = "Gửi quản lý phiếu nhập kho" },
                    new PermissionActionMetadata { Id = Permissions.Warehouse.ReceiptManagement.ApproveReject, Name = "Duyệt/Từ chối", Description = "Duyệt/Từ chối quản lý phiếu nhập kho" },
                }},
                new PermissionFeatureMetadata { Id = "Permissions.Warehouse.SupplierContractManagement", Name = "Quản lý hợp đồng NCC", Permissions = new() {
                    new PermissionActionMetadata { Id = Permissions.Warehouse.SupplierContractManagement.View, Name = "Xem", Description = "Xem quản lý hợp đồng ncc" },
                    new PermissionActionMetadata { Id = Permissions.Warehouse.SupplierContractManagement.Create, Name = "Tạo mới", Description = "Tạo mới quản lý hợp đồng ncc" },
                    new PermissionActionMetadata { Id = Permissions.Warehouse.SupplierContractManagement.Edit, Name = "Chỉnh sửa", Description = "Chỉnh sửa quản lý hợp đồng ncc" },
                    new PermissionActionMetadata { Id = Permissions.Warehouse.SupplierContractManagement.Delete, Name = "Xóa", Description = "Xóa quản lý hợp đồng ncc" },
                }},
                new PermissionFeatureMetadata { Id = "Permissions.Warehouse.SupplierManagement", Name = "Quản lý nhà cung cấp", Permissions = new() {
                    new PermissionActionMetadata { Id = Permissions.Warehouse.SupplierManagement.View, Name = "Xem", Description = "Xem quản lý nhà cung cấp" },
                    new PermissionActionMetadata { Id = Permissions.Warehouse.SupplierManagement.Create, Name = "Tạo mới", Description = "Tạo mới quản lý nhà cung cấp" },
                    new PermissionActionMetadata { Id = Permissions.Warehouse.SupplierManagement.Edit, Name = "Chỉnh sửa", Description = "Chỉnh sửa quản lý nhà cung cấp" },
                    new PermissionActionMetadata { Id = Permissions.Warehouse.SupplierManagement.Delete, Name = "Xóa", Description = "Xóa quản lý nhà cung cấp" },
                }}
            ]
        },
        new PermissionModuleMetadata
        {
            Id = "Permissions.Factory",
            Name = "Dịch Vụ & Xưởng Sửa Chữa",
            Features =
            [
                new PermissionFeatureMetadata { Id = "Permissions.Factory.CustomerManagement", Name = "Quản lý khách hàng", Permissions = new() {
                    new PermissionActionMetadata { Id = Permissions.Factory.CustomerManagement.View, Name = "Xem", Description = "Xem quản lý khách hàng" },
                    new PermissionActionMetadata { Id = Permissions.Factory.CustomerManagement.Reply, Name = "Phản hồi", Description = "Phản hồi quản lý khách hàng" },
                    new PermissionActionMetadata { Id = Permissions.Factory.CustomerManagement.EditNote, Name = "Sửa ghi chú", Description = "Sửa ghi chú quản lý khách hàng" },
                    new PermissionActionMetadata { Id = Permissions.Factory.CustomerManagement.Delete, Name = "Xóa", Description = "Xóa quản lý khách hàng" },
                }},
                new PermissionFeatureMetadata { Id = "Permissions.Factory.DashboardManagement", Name = "Tổng quan (Dashboard)", Permissions = new() {
                    new PermissionActionMetadata { Id = Permissions.Factory.DashboardManagement.View, Name = "Xem", Description = "Xem tổng quan (dashboard)" },
                    new PermissionActionMetadata { Id = Permissions.Factory.DashboardManagement.Export, Name = "Xuất dữ liệu", Description = "Xuất dữ liệu tổng quan (dashboard)" },
                }},
                new PermissionFeatureMetadata { Id = "Permissions.Factory.ContractManagement", Name = "Quản lý hợp đồng", Permissions = new() {
                    new PermissionActionMetadata { Id = Permissions.Factory.ContractManagement.View, Name = "Xem", Description = "Xem quản lý hợp đồng" },
                    new PermissionActionMetadata { Id = Permissions.Factory.ContractManagement.Create, Name = "Tạo mới", Description = "Tạo mới quản lý hợp đồng" },
                    new PermissionActionMetadata { Id = Permissions.Factory.ContractManagement.Edit, Name = "Chỉnh sửa", Description = "Chỉnh sửa quản lý hợp đồng" },
                    new PermissionActionMetadata { Id = Permissions.Factory.ContractManagement.Delete, Name = "Xóa", Description = "Xóa quản lý hợp đồng" },
                }},
                new PermissionFeatureMetadata { Id = "Permissions.Factory.CustomerSelection", Name = "Chọn khách hàng", Permissions = new() {
                    new PermissionActionMetadata { Id = Permissions.Factory.CustomerSelection.View, Name = "Xem", Description = "Xem chọn khách hàng" },
                }},
                new PermissionFeatureMetadata { Id = "Permissions.Factory.BookingManagement", Name = "Quản lý đặt lịch (Booking)", Permissions = new() {
                    new PermissionActionMetadata { Id = Permissions.Factory.BookingManagement.View, Name = "Xem", Description = "Xem quản lý đặt lịch (booking)" },
                    new PermissionActionMetadata { Id = Permissions.Factory.BookingManagement.Confirm, Name = "Xác nhận", Description = "Xác nhận quản lý đặt lịch (booking)" },
                    new PermissionActionMetadata { Id = Permissions.Factory.BookingManagement.Delete, Name = "Xóa", Description = "Xóa quản lý đặt lịch (booking)" },
                }},
                new PermissionFeatureMetadata { Id = "Permissions.Factory.RepairOrderManagement", Name = "Quản lý lệnh sửa chữa", Permissions = new() {
                    new PermissionActionMetadata { Id = Permissions.Factory.RepairOrderManagement.View, Name = "Xem", Description = "Xem quản lý lệnh sửa chữa" },
                    new PermissionActionMetadata { Id = Permissions.Factory.RepairOrderManagement.Create, Name = "Tạo mới", Description = "Tạo mới quản lý lệnh sửa chữa" },
                    new PermissionActionMetadata { Id = Permissions.Factory.RepairOrderManagement.Diagnosis, Name = "Chẩn đoán", Description = "Chẩn đoán quản lý lệnh sửa chữa" },
                    new PermissionActionMetadata { Id = Permissions.Factory.RepairOrderManagement.AssignTechnician, Name = "Phân công KTV", Description = "Phân công KTV quản lý lệnh sửa chữa" },
                    new PermissionActionMetadata { Id = Permissions.Factory.RepairOrderManagement.StartRepair, Name = "Bắt đầu sửa chữa", Description = "Bắt đầu sửa chữa quản lý lệnh sửa chữa" },
                    new PermissionActionMetadata { Id = Permissions.Factory.RepairOrderManagement.SubmitQc, Name = "Gửi kiểm tra (QC)", Description = "Gửi kiểm tra (QC) quản lý lệnh sửa chữa" },
                    new PermissionActionMetadata { Id = Permissions.Factory.RepairOrderManagement.Complete, Name = "Hoàn thành", Description = "Hoàn thành quản lý lệnh sửa chữa" },
                    new PermissionActionMetadata { Id = Permissions.Factory.RepairOrderManagement.Cancel, Name = "Hủy", Description = "Hủy quản lý lệnh sửa chữa" },
                }},
                new PermissionFeatureMetadata { Id = "Permissions.Factory.SparePartSelection", Name = "Chọn phụ tùng", Permissions = new() {
                    new PermissionActionMetadata { Id = Permissions.Factory.SparePartSelection.View, Name = "Xem", Description = "Xem chọn phụ tùng" },
                }}
            ]
        },
        new PermissionModuleMetadata
        {
            Id = "Permissions.Accountant",
            Name = "Kế Toán, Lương & Thuế",
            Features =
            [
                new PermissionFeatureMetadata { Id = "Permissions.Accountant.ContractVerification", Name = "Duyệt hợp đồng", Permissions = new() {
                    new PermissionActionMetadata { Id = Permissions.Accountant.ContractVerification.View, Name = "Xem", Description = "Xem duy?t hợp đồng" },
                }},
                new PermissionFeatureMetadata { Id = "Permissions.Accountant.DebtPaymentManagement", Name = "Quản lý công nợ", Permissions = new() {
                    new PermissionActionMetadata { Id = Permissions.Accountant.DebtPaymentManagement.View, Name = "Xem", Description = "Xem quản lý công nợ" },
                    new PermissionActionMetadata { Id = Permissions.Accountant.DebtPaymentManagement.Create, Name = "Tạo mới", Description = "Tạo mới quản lý công nợ" },
                    new PermissionActionMetadata { Id = Permissions.Accountant.DebtPaymentManagement.Edit, Name = "Chỉnh sửa", Description = "Chỉnh sửa quản lý công nợ" },
                }},
                new PermissionFeatureMetadata { Id = "Permissions.Accountant.EmployeeManagement", Name = "Quản lý nhân viên", Permissions = new() {
                    new PermissionActionMetadata { Id = Permissions.Accountant.EmployeeManagement.View, Name = "Xem", Description = "Xem quản lý nhân viên" },
                    new PermissionActionMetadata { Id = Permissions.Accountant.EmployeeManagement.Create, Name = "Tạo mới", Description = "Tạo mới quản lý nhân viên" },
                    new PermissionActionMetadata { Id = Permissions.Accountant.EmployeeManagement.Edit, Name = "Chỉnh sửa", Description = "Chỉnh sửa quản lý nhân viên" },
                    new PermissionActionMetadata { Id = Permissions.Accountant.EmployeeManagement.Delete, Name = "Xóa", Description = "Xóa quản lý nhân viên" },
                }},
                new PermissionFeatureMetadata { Id = "Permissions.Accountant.PayrollManagement", Name = "Quản lý lương", Permissions = new() {
                    new PermissionActionMetadata { Id = Permissions.Accountant.PayrollManagement.View, Name = "Xem", Description = "Xem quản lý lương" },
                    new PermissionActionMetadata { Id = Permissions.Accountant.PayrollManagement.Configure, Name = "Cấu hình", Description = "Cấu hình quản lý lương" },
                    new PermissionActionMetadata { Id = Permissions.Accountant.PayrollManagement.Approve, Name = "Phê duyệt", Description = "Phê duyệt quản lý lương" },
                }},
                new PermissionFeatureMetadata { Id = "Permissions.Accountant.SupplierContractManagement", Name = "Quản lý hợp đồng NCC", Permissions = new() {
                    new PermissionActionMetadata { Id = Permissions.Accountant.SupplierContractManagement.View, Name = "Xem", Description = "Xem quản lý hợp đồng ncc" },
                    new PermissionActionMetadata { Id = Permissions.Accountant.SupplierContractManagement.Create, Name = "Tạo mới", Description = "Tạo mới quản lý hợp đồng ncc" },
                    new PermissionActionMetadata { Id = Permissions.Accountant.SupplierContractManagement.Edit, Name = "Chỉnh sửa", Description = "Chỉnh sửa quản lý hợp đồng ncc" },
                    new PermissionActionMetadata { Id = Permissions.Accountant.SupplierContractManagement.Delete, Name = "Xóa", Description = "Xóa quản lý hợp đồng ncc" },
                }},
                new PermissionFeatureMetadata { Id = "Permissions.Accountant.CustomerManagement", Name = "Quản lý khách hàng", Permissions = new() {
                    new PermissionActionMetadata { Id = Permissions.Accountant.CustomerManagement.View, Name = "Xem", Description = "Xem quản lý khách hàng" },
                    new PermissionActionMetadata { Id = Permissions.Accountant.CustomerManagement.Reply, Name = "Phản hồi", Description = "Phản hồi quản lý khách hàng" },
                    new PermissionActionMetadata { Id = Permissions.Accountant.CustomerManagement.EditNote, Name = "Sửa ghi chú", Description = "Sửa ghi chú quản lý khách hàng" },
                    new PermissionActionMetadata { Id = Permissions.Accountant.CustomerManagement.Delete, Name = "Xóa", Description = "Xóa quản lý khách hàng" },
                }},
                new PermissionFeatureMetadata { Id = "Permissions.Accountant.DashboardManagement", Name = "Tổng quan (Dashboard)", Permissions = new() {
                    new PermissionActionMetadata { Id = Permissions.Accountant.DashboardManagement.View, Name = "Xem", Description = "Xem tổng quan (dashboard)" },
                    new PermissionActionMetadata { Id = Permissions.Accountant.DashboardManagement.Export, Name = "Xuất dữ liệu", Description = "Xuất dữ liệu tổng quan (dashboard)" },
                }},
                new PermissionFeatureMetadata { Id = "Permissions.Accountant.ContractManagement", Name = "Quản lý hợp đồng", Permissions = new() {
                    new PermissionActionMetadata { Id = Permissions.Accountant.ContractManagement.View, Name = "Xem", Description = "Xem quản lý hợp đồng" },
                    new PermissionActionMetadata { Id = Permissions.Accountant.ContractManagement.Create, Name = "Tạo mới", Description = "Tạo mới quản lý hợp đồng" },
                    new PermissionActionMetadata { Id = Permissions.Accountant.ContractManagement.Edit, Name = "Chỉnh sửa", Description = "Chỉnh sửa quản lý hợp đồng" },
                    new PermissionActionMetadata { Id = Permissions.Accountant.ContractManagement.Delete, Name = "Xóa", Description = "Xóa quản lý hợp đồng" },
                }}
            ]
        },
        new PermissionModuleMetadata
        {
            Id = "Permissions.Order",
            Name = "Đơn hàng & Vận chuyển",
            Features =
            [
                new PermissionFeatureMetadata { Id = "Permissions.Order.ProductManagement", Name = "Quản lý sản phẩm", Permissions = new() {
                    new PermissionActionMetadata { Id = Permissions.Order.ProductManagement.View, Name = "Xem", Description = "Xem quản lý sản phẩm" },
                    new PermissionActionMetadata { Id = Permissions.Order.ProductManagement.Create, Name = "Tạo mới", Description = "Tạo mới quản lý sản phẩm" },
                    new PermissionActionMetadata { Id = Permissions.Order.ProductManagement.Edit, Name = "Chỉnh sửa", Description = "Chỉnh sửa quản lý sản phẩm" },
                    new PermissionActionMetadata { Id = Permissions.Order.ProductManagement.Delete, Name = "Xóa", Description = "Xóa quản lý sản phẩm" },
                    new PermissionActionMetadata { Id = Permissions.Order.ProductManagement.EditPrice, Name = "Chỉnh giá", Description = "Chỉnh giá quản lý sản phẩm" },
                    new PermissionActionMetadata { Id = Permissions.Order.ProductManagement.ChangeStatus, Name = "Đổi trạng thái", Description = "Đổi trạng thái quản lý sản phẩm" },
                }},
                new PermissionFeatureMetadata { Id = "Permissions.Order.CustomerManagement", Name = "Quản lý khách hàng", Permissions = new() {
                    new PermissionActionMetadata { Id = Permissions.Order.CustomerManagement.View, Name = "Xem", Description = "Xem quản lý khách hàng" },
                    new PermissionActionMetadata { Id = Permissions.Order.CustomerManagement.Reply, Name = "Phản hồi", Description = "Phản hồi quản lý khách hàng" },
                    new PermissionActionMetadata { Id = Permissions.Order.CustomerManagement.EditNote, Name = "Sửa ghi chú", Description = "Sửa ghi chú quản lý khách hàng" },
                    new PermissionActionMetadata { Id = Permissions.Order.CustomerManagement.Delete, Name = "Xóa", Description = "Xóa quản lý khách hàng" },
                }},
                new PermissionFeatureMetadata { Id = "Permissions.Order.ContractManagement", Name = "Quản lý hợp đồng", Permissions = new() {
                    new PermissionActionMetadata { Id = Permissions.Order.ContractManagement.View, Name = "Xem", Description = "Xem quản lý hợp đồng" },
                    new PermissionActionMetadata { Id = Permissions.Order.ContractManagement.Create, Name = "Tạo mới", Description = "Tạo mới quản lý hợp đồng" },
                    new PermissionActionMetadata { Id = Permissions.Order.ContractManagement.Edit, Name = "Chỉnh sửa", Description = "Chỉnh sửa quản lý hợp đồng" },
                    new PermissionActionMetadata { Id = Permissions.Order.ContractManagement.Delete, Name = "Xóa", Description = "Xóa quản lý hợp đồng" },
                }},
                new PermissionFeatureMetadata { Id = "Permissions.Order.CustomerSelection", Name = "Chọn khách hàng", Permissions = new() {
                    new PermissionActionMetadata { Id = Permissions.Order.CustomerSelection.View, Name = "Xem", Description = "Xem chọn khách hàng" },
                }},
                new PermissionFeatureMetadata { Id = "Permissions.Order.OrderManagement", Name = "Quản lý đơn hàng", Permissions = new() {
                    new PermissionActionMetadata { Id = Permissions.Order.OrderManagement.View, Name = "Xem", Description = "Xem quản lý đơn hàng" },
                    new PermissionActionMetadata { Id = Permissions.Order.OrderManagement.Create, Name = "Tạo mới", Description = "Tạo mới quản lý đơn hàng" },
                    new PermissionActionMetadata { Id = Permissions.Order.OrderManagement.Edit, Name = "Chỉnh sửa", Description = "Chỉnh sửa quản lý đơn hàng" },
                    new PermissionActionMetadata { Id = Permissions.Order.OrderManagement.Delete, Name = "Xóa", Description = "Xóa quản lý đơn hàng" },
                    new PermissionActionMetadata { Id = Permissions.Order.OrderManagement.ChangeStatus, Name = "Đổi trạng thái", Description = "Đổi trạng thái quản lý đơn hàng" },
                }},
                new PermissionFeatureMetadata { Id = "Permissions.Order.ProductSelection", Name = "Chọn sản phẩm", Permissions = new() {
                    new PermissionActionMetadata { Id = Permissions.Order.ProductSelection.View, Name = "Xem", Description = "Xem chọn sản phẩm" },
                }}
            ]
        }
    ];

    public static List<string> GetAllPermissions()
    {
        var modules = ModulesTree.Select(m => m.Id);
        var actions = ModulesTree.SelectMany(m => m.Features.SelectMany(f => f.Permissions.Select(a => a.Id)));
        return modules.Concat(actions).ToList();
    }

    public static (bool IsValid, string ErrorMessage) ValidateRules(List<string> permissions)
    {
        var validPermissions = GetAllPermissions();
        var invalidPermissions = permissions.Where(p => !validPermissions.Contains(p)).ToList();
        if (invalidPermissions.Count != 0)
            return (false, $"Invalid permissions: {string.Join(", ", invalidPermissions)}");

        foreach (var p in permissions)
        {
            if (!p.EndsWith(".View"))
            {
                var lastDotIndex = p.LastIndexOf('.');
                if (lastDotIndex > 0)
                {
                    var viewPerm = string.Concat(p.AsSpan(0, lastDotIndex), ".View");
                    if (validPermissions.Contains(viewPerm) && !permissions.Contains(viewPerm))
                    {
                        return (false, $"Permission '{p}' requires: {viewPerm}");
                    }
                }
            }
        }
        return (true, string.Empty);
    }

    public static List<PermissionActionMetadata> GetMetadataList()
    {
        return ModulesTree.SelectMany(m => m.Features.SelectMany(f => f.Permissions)).ToList();
    }
    
    public static List<PermissionActionMetadata> GetMetadataList(IEnumerable<string> permissions)
    {
        return ModulesTree.SelectMany(m => m.Features.SelectMany(f => f.Permissions)).Where(p => permissions.Contains(p.Id)).ToList();
    }

    public static PermissionActionMetadata? GetMetadata(string id)
    {
        return ModulesTree.SelectMany(m => m.Features.SelectMany(f => f.Permissions)).FirstOrDefault(p => p.Id == id);
    }

    public static readonly Dictionary<string, List<string>> Conflicts = [];
    public static readonly Dictionary<string, List<string>> Dependencies = GenerateDependencies();

    private static Dictionary<string, List<string>> GenerateDependencies()
    {
        var dict = new Dictionary<string, List<string>>();
        var validPermissions = GetMetadataList().Select(m => m.Id).ToHashSet();
        foreach (var p in validPermissions)
        {
            if (!p.EndsWith(".View"))
            {
                var lastDotIndex = p.LastIndexOf('.');
                if (lastDotIndex > 0)
                {
                    var viewPerm = string.Concat(p.AsSpan(0, lastDotIndex), ".View");
                    if (validPermissions.Contains(viewPerm))
                    {
                        dict[p] = [viewPerm];
                    }
                }
            }
        }
        return dict;
    }
}
