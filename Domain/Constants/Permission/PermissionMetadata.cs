namespace Domain.Constants.Security
{
    /// <summary>
    /// Metadata của một permission
    /// </summary>
    public class PermissionMetadata(string displayName, string description)
    {
        /// <summary>
        /// Tên hiển thị của permission
        /// </summary>
        public string DisplayName { get; } = displayName;

        /// <summary>
        /// Mô tả chi tiết của permission
        /// </summary>
        public string Description { get; } = description;
    }
}
