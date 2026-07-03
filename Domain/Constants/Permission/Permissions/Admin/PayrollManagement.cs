namespace Domain.Constants.Permission;

public static partial class Permissions
{
    public static partial class Admin 
    {
        public static class PayrollManagement 
        {
            public const string View = "Permissions.Admin.PayrollManagement.View";
            public const string Configure = "Permissions.Admin.PayrollManagement.Configure";
            public const string Approve = "Permissions.Admin.PayrollManagement.Approve";
        }
    }
}
