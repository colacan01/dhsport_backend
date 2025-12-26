using AutoMapper;
using DhSport.Application.DTOs.Post;
using DhSport.Domain.Entities.Content;

namespace DhSport.Application.Mappings;

public class PostProfile : Profile
{
    public PostProfile()
    {
        CreateMap<Post, PostDto>()
            .ForMember(dest => dest.BoardNm, opt => opt.Ignore())
            .ForMember(dest => dest.PostTypeNm, opt => opt.Ignore())
            .ForMember(dest => dest.AuthorNm, opt => opt.Ignore());

        CreateMap<Post, PostDetailDto>()
            .ForMember(dest => dest.BoardNm, opt => opt.Ignore())
            .ForMember(dest => dest.PostTypeNm, opt => opt.Ignore())
            .ForMember(dest => dest.AuthorNm, opt => opt.Ignore())
            .ForMember(dest => dest.Files, opt => opt.Ignore())
            .ForMember(dest => dest.Comments, opt => opt.Ignore());

        CreateMap<PostFile, PostFileDto>();

        CreateMap<Comment, CommentDto>()
            .ForMember(dest => dest.AuthorNm, opt => opt.Ignore())
            .ForMember(dest => dest.Replies, opt => opt.Ignore());

        CreateMap<PostType, PostTypeDto>();
    }
}
