using AutoMapper;
using DhSport.Application.DTOs.User;
using DhSport.Application.Interfaces;
using DhSport.Domain.Entities.UserManagement;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DhSport.Application.Features.Auth.Commands.Login;

public class LoginCommandHandler : IRequestHandler<LoginCommand, LoginResponseDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IJwtService _jwtService;
    private readonly IMapper _mapper;

    public LoginCommandHandler(IUnitOfWork unitOfWork, IJwtService jwtService, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _jwtService = jwtService;
        _mapper = mapper;
    }

    public async Task<LoginResponseDto> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var user = await _unitOfWork.Repository<User>()
            .GetQueryable()
            .FirstOrDefaultAsync(u => u.LogonId == request.Dto.LogonId && !u.IsDeleted, cancellationToken);

        if (user == null || !BCrypt.Net.BCrypt.Verify(request.Dto.Passwd, user.Passwd))
            throw new UnauthorizedAccessException("Invalid credentials");

        if (!user.IsActive)
            throw new UnauthorizedAccessException("User account is inactive");

        // Get user roles
        var userRoles = await _unitOfWork.Repository<UserRoleMap>()
            .GetQueryable()
            .Where(ur => ur.UserId == user.Id && ur.IsActive)
            .Include(ur => ur.Role)
            .ToListAsync(cancellationToken);

        var roles = userRoles.Select(ur => ur.Role.RoleNm).ToList();

        // Generate JWT token
        var token = _jwtService.GenerateToken(user, roles);
        var expiresAt = DateTime.UtcNow.AddHours(24); // Match with JWT expiration

        // Update last login time
        user.LastLoginDttm = DateTime.UtcNow;
        user.UpdateDttm = DateTime.UtcNow;
        await _unitOfWork.Repository<User>().UpdateAsync(user, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var userDto = _mapper.Map<UserDto>(user);
        userDto.Roles = roles;

        return new LoginResponseDto
        {
            Token = token,
            User = userDto,
            ExpiresAt = expiresAt
        };
    }
}
