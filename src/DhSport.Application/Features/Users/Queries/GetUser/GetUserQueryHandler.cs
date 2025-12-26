using AutoMapper;
using DhSport.Application.DTOs.User;
using DhSport.Application.Interfaces;
using DhSport.Domain.Entities.UserManagement;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DhSport.Application.Features.Users.Queries.GetUser;

public class GetUserQueryHandler : IRequestHandler<GetUserQuery, UserDto?>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetUserQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<UserDto?> Handle(GetUserQuery request, CancellationToken cancellationToken)
    {
        var user = await _unitOfWork.Repository<User>()
            .GetByIdAsync(request.Id, cancellationToken);

        if (user == null)
            return null;

        var userDto = _mapper.Map<UserDto>(user);

        // Get user roles
        var userRoles = await _unitOfWork.Repository<UserRoleMap>()
            .GetQueryable()
            .Where(ur => ur.UserId == user.Id && ur.IsActive)
            .Include(ur => ur.Role)
            .ToListAsync(cancellationToken);

        userDto.Roles = userRoles.Select(ur => ur.Role.RoleNm).ToList();

        return userDto;
    }
}
