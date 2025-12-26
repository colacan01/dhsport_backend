using DhSport.Domain.Entities.UserManagement;

namespace DhSport.Application.Interfaces;

public interface IJwtService
{
    string GenerateToken(User user, List<string> roles);
    Guid? ValidateToken(string token);
}
