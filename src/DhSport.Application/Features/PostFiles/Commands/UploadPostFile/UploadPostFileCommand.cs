using DhSport.Application.DTOs.Post;
using DhSport.Application.DTOs.PostFile;
using MediatR;

namespace DhSport.Application.Features.PostFiles.Commands.UploadPostFile;

public record UploadPostFileCommand(Guid PostId, List<UploadPostFileDto> Files) : IRequest<List<PostFileDto>>;
