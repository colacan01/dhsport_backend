namespace DhSport.API.DTOs;

public class FeatureTypeDto
{
    public Guid Id { get; set; }
    public string FeatureTypeNm { get; set; } = string.Empty;
    public string? FeatureTypeDesc { get; set; }
    public bool IsActive { get; set; }
    public int DisplayOrder { get; set; }
    public DateTime CreateDttm { get; set; }
    public DateTime? UpdateDttm { get; set; }
}
