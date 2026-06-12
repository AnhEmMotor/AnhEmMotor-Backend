using System.ComponentModel.DataAnnotations;

namespace Application.ApiContracts.ServiceCategory.Requests;

public class CreateServiceCategoryRequest
{
    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;

    [StringLength(100)]
    public string? Slug { get; set; }

    [StringLength(500)]
    public string? Description { get; set; }

    public int? ParentId { get; set; }

    public bool IsActive { get; set; } = true;
}
