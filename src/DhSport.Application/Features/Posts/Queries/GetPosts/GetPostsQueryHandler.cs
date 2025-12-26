using AutoMapper;
using DhSport.Application.DTOs.Post;
using DhSport.Application.Interfaces;
using DhSport.Domain.Entities.Content;
using DhSport.Domain.Entities.UserManagement;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DhSport.Application.Features.Posts.Queries.GetPosts;

public class GetPostsQueryHandler : IRequestHandler<GetPostsQuery, List<PostDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetPostsQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<List<PostDto>> Handle(GetPostsQuery request, CancellationToken cancellationToken)
    {
        var query = _unitOfWork.Repository<Post>()
            .GetQueryable()
            .Where(p => !p.IsDeleted);

        if (request.BoardId.HasValue)
            query = query.Where(p => p.BoardId == request.BoardId.Value);

        if (request.PostTypeId.HasValue)
            query = query.Where(p => p.PostTypeId == request.PostTypeId.Value);

        if (!string.IsNullOrEmpty(request.SearchKeyword))
        {
            // Search in title and content (JSONB is converted to text for searching)
            query = query.Where(p =>
                p.Title.Contains(request.SearchKeyword) ||
                p.PostContent!.ToString()!.Contains(request.SearchKeyword));
        }

        var posts = await query
            .OrderByDescending(p => p.IsNotice)
            .ThenByDescending(p => p.CreateDttm)
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);

        var postDtos = _mapper.Map<List<PostDto>>(posts);

        // Get related data in batch
        var boardIds = posts.Select(p => p.BoardId).Distinct().ToList();
        var postTypeIds = posts.Select(p => p.PostTypeId).Distinct().ToList();
        var authorIds = posts.Select(p => p.AuthorId).Distinct().ToList();

        var boards = await _unitOfWork.Repository<Board>()
            .GetQueryable()
            .Where(b => boardIds.Contains(b.Id))
            .ToListAsync(cancellationToken);

        var postTypes = await _unitOfWork.Repository<PostType>()
            .GetQueryable()
            .Where(pt => postTypeIds.Contains(pt.Id))
            .ToListAsync(cancellationToken);

        var authors = await _unitOfWork.Repository<User>()
            .GetQueryable()
            .Where(u => authorIds.Contains(u.Id))
            .ToListAsync(cancellationToken);

        var boardDict = boards.ToDictionary(b => b.Id, b => b.BoardNm);
        var postTypeDict = postTypes.ToDictionary(pt => pt.Id, pt => pt.PostTypeNm);
        var authorDict = authors.ToDictionary(a => a.Id, a => a.UserNm);

        foreach (var postDto in postDtos)
        {
            if (boardDict.TryGetValue(postDto.BoardId, out var boardNm))
                postDto.BoardNm = boardNm;

            if (postTypeDict.TryGetValue(postDto.PostTypeId, out var postTypeNm))
                postDto.PostTypeNm = postTypeNm;

            if (authorDict.TryGetValue(postDto.AuthorId, out var authorNm))
                postDto.AuthorNm = authorNm;
        }

        return postDtos;
    }
}
