using AutoMapper;
using DhSport.Application.DTOs.Auth;
using DhSport.Application.Interfaces;
using DhSport.Domain.Entities.UserManagement;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DhSport.Application.Features.Auth.Commands.Register;

public class RegisterCommandHandler : IRequestHandler<RegisterCommand, RegisterResponseDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public RegisterCommandHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<RegisterResponseDto> Handle(RegisterCommand request, CancellationToken cancellationToken)
    {
        // 1. Check for duplicate LogonId
        var existingUserByLogonId = await _unitOfWork.Repository<User>()
            .GetQueryable()
            .FirstOrDefaultAsync(u => u.LogonId.ToLower() == request.Dto.LogonId.ToLower(), cancellationToken);

        if (existingUserByLogonId != null)
        {
            throw new InvalidOperationException($"로그인 ID '{request.Dto.LogonId}'는 이미 사용 중입니다.");
        }

        // 2. Check for duplicate Email
        var existingUserByEmail = await _unitOfWork.Repository<User>()
            .GetQueryable()
            .FirstOrDefaultAsync(u => u.Email.ToLower() == request.Dto.Email.ToLower(), cancellationToken);

        if (existingUserByEmail != null)
        {
            throw new InvalidOperationException($"이메일 '{request.Dto.Email}'은 이미 사용 중입니다.");
        }

        // 3. Hash password
        var hashedPassword = BCrypt.Net.BCrypt.HashPassword(request.Dto.Passwd);

        // 4. Create User entity
        var user = _mapper.Map<User>(request.Dto);
        user.Id = Guid.NewGuid();
        user.Passwd = hashedPassword;
        user.IsActive = true;
        user.IsDeleted = false;
        user.CreateDttm = DateTime.UtcNow;
        user.UpdateDttm = DateTime.UtcNow;

        await _unitOfWork.Repository<User>().AddAsync(user, cancellationToken);

        // 5. Assign "회원" role automatically
        var memberRole = await _unitOfWork.Repository<Role>()
            .GetQueryable()
            .FirstOrDefaultAsync(r => r.RoleNm == "회원" && r.IsActive, cancellationToken);

        if (memberRole == null)
        {
            throw new InvalidOperationException("'회원' 역할이 데이터베이스에 존재하지 않습니다. 데이터베이스 시딩을 확인하세요.");
        }

        var userRoleMap = new UserRoleMap
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            RoleId = memberRole.Id,
            IsActive = true,
            CreateDttm = DateTime.UtcNow,
            UpdateDttm = DateTime.UtcNow
        };

        await _unitOfWork.Repository<UserRoleMap>().AddAsync(userRoleMap, cancellationToken);

        // 6. Save changes
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // 7. Return response DTO
        var response = _mapper.Map<RegisterResponseDto>(user);
        return response;
    }
}
