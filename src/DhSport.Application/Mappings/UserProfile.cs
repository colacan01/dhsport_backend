using AutoMapper;
using DhSport.Application.DTOs.Auth;
using DhSport.Application.DTOs.User;
using DhSport.Domain.Entities.UserManagement;

namespace DhSport.Application.Mappings;

public class UserProfile : Profile
{
    public UserProfile()
    {
        CreateMap<User, UserDto>()
            .ForMember(dest => dest.Roles, opt => opt.Ignore());

        CreateMap<CreateUserDto, User>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.CreateDttm, opt => opt.Ignore())
            .ForMember(dest => dest.CreateUserId, opt => opt.Ignore())
            .ForMember(dest => dest.UpdateDttm, opt => opt.Ignore())
            .ForMember(dest => dest.UpdateUserId, opt => opt.Ignore())
            .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
            .ForMember(dest => dest.LastLoginDttm, opt => opt.Ignore())
            .ForMember(dest => dest.UserRoles, opt => opt.Ignore());

        // RegisterDto → User
        CreateMap<RegisterDto, User>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.Passwd, opt => opt.Ignore()) // Hashed in handler
            .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => true))
            .ForMember(dest => dest.IsDeleted, opt => opt.MapFrom(src => false))
            .ForMember(dest => dest.LastLoginDttm, opt => opt.Ignore())
            .ForMember(dest => dest.CreateDttm, opt => opt.Ignore())
            .ForMember(dest => dest.CreateUserId, opt => opt.Ignore())
            .ForMember(dest => dest.UpdateDttm, opt => opt.Ignore())
            .ForMember(dest => dest.UpdateUserId, opt => opt.Ignore())
            .ForMember(dest => dest.UserRoles, opt => opt.Ignore());

        // User → RegisterResponseDto
        CreateMap<User, RegisterResponseDto>()
            .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.Id));
    }
}
