using AutoMapper;
using DhSport.Application.DTOs.User;
using DhSport.Application.Interfaces;
using DhSport.Domain.Entities.UserManagement;
using MediatR;

namespace DhSport.Application.Features.Users.Commands.UpdateUser;

public class UpdateUserCommandHandler : IRequestHandler<UpdateUserCommand, UserDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public UpdateUserCommandHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<UserDto> Handle(UpdateUserCommand request, CancellationToken cancellationToken)
    {
        var user = await _unitOfWork.Repository<User>().GetByIdAsync(request.Id, cancellationToken);

        if (user == null)
            throw new KeyNotFoundException($"User with ID {request.Id} not found");

        if (!string.IsNullOrEmpty(request.Dto.UserNm))
            user.UserNm = request.Dto.UserNm;

        if (!string.IsNullOrEmpty(request.Dto.Email))
            user.Email = request.Dto.Email;

        if (request.Dto.Tel != null)
            user.Tel = request.Dto.Tel;

        if (request.Dto.ProfileImg != null)
            user.ProfileImg = request.Dto.ProfileImg;

        if (request.Dto.IsActive.HasValue)
            user.IsActive = request.Dto.IsActive.Value;

        user.UpdateDttm = DateTime.UtcNow;

        await _unitOfWork.Repository<User>().UpdateAsync(user, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return _mapper.Map<UserDto>(user);
    }
}
