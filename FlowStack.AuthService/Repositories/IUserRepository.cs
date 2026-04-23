using FlowStack.AuthService.Models;

namespace FlowStack.AuthService.Repositories;

public interface IUserRepository
{
    Task<User?> FindByEmailAsync(string email);
    Task<User?> FindByUsernameAsync(string username);
    Task<User?> FindByUserIdAsync(Guid userId);
    Task<User?> FindByProviderAsync(OAuthProvider provider, string providerUserId);
    Task<bool>  ExistsByEmailAsync(string email);
    Task<bool>  ExistsByUsernameAsync(string username);
    Task<IEnumerable<User>> FindAllByRoleAsync(UserRole role);
    Task<IEnumerable<User>> SearchByFullNameAsync(string query);
    Task<User>  CreateAsync(User user);
    Task<User>  UpdateAsync(User user);
    Task DeleteByUserIdAsync(Guid userId);

    //bypasses IsActive query filter
    Task<User?> FindByUserIdIncludingInactiveAsync(Guid userId);
    Task<IEnumerable<User>> GetAllUsersAsync(int page, int pageSize);
}