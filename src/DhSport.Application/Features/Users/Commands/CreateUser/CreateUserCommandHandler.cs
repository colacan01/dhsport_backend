using AutoMapper;
using DhSport.Application.DTOs.User;
using DhSport.Application.Interfaces;
using DhSport.Domain.Entities.UserManagement;
using MediatR;

namespace DhSport.Application.Features.Users.Commands.CreateUser;

public class CreateUserCommandHandler : IRequestHandler<CreateUserCommand, UserDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public CreateUserCommandHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<UserDto> Handle(CreateUserCommand request, CancellationToken cancellationToken)
    {
        var user = new User
        {
            LogonId = request.Dto.LogonId,
            Passwd = BCrypt.Net.BCrypt.HashPassword(request.Dto.Passwd),
            UserNm = request.Dto.UserNm,
            Email = request.Dto.Email,
            Tel = request.Dto.Tel,
            ProfileImg = request.Dto.ProfileImg,
            IsActive = true,
            CreateDttm = DateTime.UtcNow
        };

        await _unitOfWork.Repository<User>().AddAsync(user, cancellationToken);

        // Add user roles if provided
        if (request.Dto.RoleIds?.Any() == true)
        {
            foreach (var roleId in request.Dto.RoleIds)
            {
                var userRole = new UserRoleMap
                {
                    UserId = user.Id,
                    RoleId = roleId,
                    IsActive = true,
                    CreateDttm = DateTime.UtcNow
                };
                await _unitOfWork.Repository<UserRoleMap>().AddAsync(userRole, cancellationToken);
            }
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return _mapper.Map<UserDto>(user);
    }
}
