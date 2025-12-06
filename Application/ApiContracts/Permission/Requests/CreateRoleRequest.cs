using System;
using System.Collections.Generic;
using System.Text;

namespace Application.ApiContracts.Permission.Requests
{
    /// <summary>
    /// DTO cho tạo role mới
    /// </summary>
    public class CreateRoleRequest
    {
        /// <summary>
        /// Tên vai trò
        /// </summary>
        public string RoleName { get; set; } = string.Empty;

        /// <summary>
        /// Mô tả của vai trò (tuỳ chọn)
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// Danh sách quyền cho vai trò (bắt buộc - phải có ít nhất 1 quyền)
        /// </summary>
        public List<string> Permissions { get; set; } = [];
    }
}
