namespace DhSport.API.DTOs;

public class MenuDto
{
    public Guid Id { get; set; }
    public Guid? ParentMenuId { get; set; }
    public string MenuNm { get; set; } = string.Empty;
    public string? MenuUrl { get; set; }
    public string? MenuIcon { get; set; }
    public int DisplayOrder { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreateDttm { get; set; }
    public DateTime? UpdateDttm { get; set; }

    // 계층 구조 정보
    public MenuBasicDto? ParentMenu { get; set; }
    public List<MenuBasicDto>? ChildMenus { get; set; }
}
