using AutoMapper;
using DhSport.Application.DTOs.Board;
using DhSport.Domain.Entities.Content;

namespace DhSport.Application.Mappings;

public class BoardProfile : Profile
{
    public BoardProfile()
    {
        CreateMap<Board, BoardDto>()
            .ForMember(dest => dest.BoardTypeNm, opt => opt.Ignore())
            .ForMember(dest => dest.PostCount, opt => opt.Ignore());

        CreateMap<BoardType, BoardTypeDto>();
    }
}
