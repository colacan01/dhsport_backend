using AutoMapper;
using DhSport.Application.DTOs.Post;
using DhSport.Application.Interfaces;
using DhSport.Domain.Entities.Content;
using MediatR;

namespace DhSport.Application.Features.PostFiles.Commands.UploadPostFile;

public class UploadPostFileCommandHandler : IRequestHandler<UploadPostFileCommand, List<PostFileDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public UploadPostFileCommandHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<List<PostFileDto>> Handle(UploadPostFileCommand request, CancellationToken cancellationToken)
    {
        var post = await _unitOfWork.Repository<Post>().GetByIdAsync(request.PostId, cancellationToken);
        if (post == null)
            throw new KeyNotFoundException($"Post with ID {request.PostId} not found");

        var uploadedFiles = new List<PostFileDto>();
        var displayOrder = 0;

        foreach (var fileDto in request.Files)
        {
            var postFile = new PostFile
            {
                PostId = request.PostId,
                PostFilePath = fileDto.FilePath,
                PostFileNm = fileDto.FileName,
                PostFileType = fileDto.ContentType,
                PostFileSize = fileDto.FileSize,
                DisplayOrder = displayOrder++,
                CreateDttm = DateTime.UtcNow
            };

            await _unitOfWork.Repository<PostFile>().AddAsync(postFile, cancellationToken);

            var dto = _mapper.Map<PostFileDto>(postFile);
            uploadedFiles.Add(dto);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return uploadedFiles;
    }
}
