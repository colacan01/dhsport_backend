using DhSport.Application.Interfaces;
using DhSport.Domain.Entities.UserManagement;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DhSport.Application.Features.Auth.Commands.Logout;

public class LogoutCommandHandler : IRequestHandler<LogoutCommand, Unit>
{
    private readonly IUnitOfWork _unitOfWork;

    public LogoutCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Unit> Handle(LogoutCommand request, CancellationToken cancellationToken)
    {
        // 1. Find user
        var user = await _unitOfWork.Repository<User>()
            .GetQueryable()
            .FirstOrDefaultAsync(u => u.Id == request.UserId && !u.IsDeleted, cancellationToken);

        if (user == null)
        {
            throw new InvalidOperationException("사용자를 찾을 수 없습니다.");
        }

        // 2. Set LastLoginDttm to null (indicate logout)
        user.LastLoginDttm = null;
        user.UpdateDttm = DateTime.UtcNow;

        await _unitOfWork.Repository<User>().UpdateAsync(user, cancellationToken);

        // 3. Save changes
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // 4. Return empty response
        return Unit.Value;
    }
}
