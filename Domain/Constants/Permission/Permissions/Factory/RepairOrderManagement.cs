namespace Domain.Constants.Permission;

public static partial class Permissions
{
    public static partial class Factory 
    {
        public static class RepairOrderManagement 
        {
            public const string View = "Permissions.Factory.RepairOrderManagement.View";
            public const string Create = "Permissions.Factory.RepairOrderManagement.Create";
            public const string Diagnosis = "Permissions.Factory.RepairOrderManagement.Diagnosis";
            public const string AssignTechnician = "Permissions.Factory.RepairOrderManagement.AssignTechnician";
            public const string StartRepair = "Permissions.Factory.RepairOrderManagement.StartRepair";
            public const string SubmitQc = "Permissions.Factory.RepairOrderManagement.SubmitQc";
            public const string Complete = "Permissions.Factory.RepairOrderManagement.Complete";
            public const string Cancel = "Permissions.Factory.RepairOrderManagement.Cancel";
        }
    }
}
