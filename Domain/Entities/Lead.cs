using Domain.Constants.Lead;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities;

[Table("Lead")]
public class Lead : BaseEntity
{
    [Key]
    [Column("Id")]
    public int Id { get; set; }

    [Column("FullName", TypeName = "nvarchar(100)")]
    public string FullName { get; set; } = string.Empty;

    [Column("Email", TypeName = "nvarchar(100)")]
    public string Email { get; set; } = string.Empty;

    [Column("PhoneNumber", TypeName = "nvarchar(20)")]
    public string PhoneNumber { get; set; } = string.Empty;

    [Column("Score")]
    public int Score { get; set; } = 0;

    [Column("Status", TypeName = "nvarchar(20)")]
    public string Status { get; set; } = LeadStatus.New;

    [Column("Source", TypeName = "nvarchar(50)")]
    public string Source { get; set; } = LeadSource.WebStore;

    [Column("InterestedVehicle", TypeName = "nvarchar(255)")]
    public string InterestedVehicle { get; set; } = string.Empty;

    [Column("Notes", TypeName = "nvarchar(MAX)")]
    public string Notes { get; set; } = string.Empty;

    [Column("Priority", TypeName = "nvarchar(20)")]
    public string Priority { get; set; } = string.Empty;

    [Column("Address", TypeName = "nvarchar(500)")]
    public string Address { get; set; } = string.Empty;

    [Column("AddressDetail", TypeName = "nvarchar(500)")]
    public string AddressDetail { get; set; } = string.Empty;

    [Column("Ward", TypeName = "nvarchar(100)")]
    public string Ward { get; set; } = string.Empty;

    [Column("District", TypeName = "nvarchar(100)")]
    public string District { get; set; } = LeadAddressDefaults.District;

    [Column("Province", TypeName = "nvarchar(100)")]
    public string Province { get; set; } = LeadAddressDefaults.Province;

    [Column("Gender", TypeName = "nvarchar(20)")]
    public string Gender { get; set; } = string.Empty;

    [Column("Birthday")]
    public DateTime? Birthday { get; set; }

    [Column("IdentificationNumber", TypeName = "nvarchar(20)")]
    public string IdentificationNumber { get; set; } = string.Empty;

[Column("Tier", TypeName = "nvarchar(50)")]
public string Tier { get; set; } = LeadTier.NewMember;

[Column("Points")]
public int Points { get; set; } = 0;

[Column("IsVerified")]
public bool IsVerified { get; set; } = false;

[Column("AssignedToId")]
    public Guid? AssignedToId { get; set; }

    [ForeignKey("AssignedToId")]
    public ApplicationUser? AssignedTo { get; set; }

    public ICollection<LeadActivity> Activities { get; set; } = [];
}
