namespace Domain.Entities;

public class ServiceCategory : BaseEntity
{
    public int Id { get; set; }

    public required string Name { get; set; }

    public string? Description { get; set; }

    public ICollection<Service> Services { get; set; } = new List<Service>();
}