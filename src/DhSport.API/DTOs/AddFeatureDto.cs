namespace DhSport.API.DTOs;

public class AddFeatureDto
{
    public Guid Id { get; set; }
    public Guid PostId { get; set; }
    public Guid FeatureTypeId { get; set; }
    public string FeatureContent { get; set; } = string.Empty;
    public bool IsDeleted { get; set; }
    public DateTime CreateDttm { get; set; }
    public DateTime? UpdateDttm { get; set; }

    // Nested FeatureType information
    public FeatureTypeDto? FeatureType { get; set; }
}
