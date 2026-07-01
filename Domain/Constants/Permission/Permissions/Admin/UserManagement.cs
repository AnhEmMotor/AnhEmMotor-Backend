namespace Domain.Constants.Permission;

public static partial class Permissions
{
    public static partial class Admin 
    {
        public static class UserManagement 
        {
            public const string View = "Permissions.Admin.UserManagement.View";
            public const string Create = "Permissions.Admin.UserManagement.Create";
            public const string Edit = "Permissions.Admin.UserManagement.Edit";
            public const string Delete = "Permissions.Admin.UserManagement.Delete";
            public const string AssignRoles = "Permissions.Admin.UserManagement.AssignRoles";
            public const string ChangePassword = "Permissions.Admin.UserManagement.ChangePassword";
        }
    }
}
