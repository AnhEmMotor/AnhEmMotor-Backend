namespace Application.ApiContracts.VehicleType.Responses
{
    public class VehicleTypeResponse
    {
        public int? Id { get; set; }
        public string? Name { get; set; }
        public string? Slug { get; set; }
        public string? ImageUrl { get; set; }
        public bool IsActive { get; set; }
        public int SortOrder { get; set; }
        public string? Description { get; set; }
        public int ProductCount { get; set; }
    }
}
