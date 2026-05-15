using FlowStack.AuthService.Data;
using FlowStack.AuthService.Models;
using Microsoft.EntityFrameworkCore;

namespace FlowStack.AuthService.Repositories;

public class UserRepository : IUserRepository
{
    private readonly AuthDbContext _db;

    public UserRepository(AuthDbContext db)
    {
        _db = db;
    }

    public async Task<User?> FindByEmailAsync(string email) =>
        await _db.Users.FirstOrDefaultAsync(u => u.Email == email.ToLower());

    public async Task<User?> FindByUsernameAsync(string username) =>
        await _db.Users.FirstOrDefaultAsync(u => u.Username == username.ToLower());

    public async Task<User?> FindByUserIdAsync(Guid userId) =>
        await _db.Users.FirstOrDefaultAsync(u => u.UserId == userId);

    public async Task<User?> FindByProviderAsync(OAuthProvider provider, string providerUserId) =>
        await _db.Users.FirstOrDefaultAsync(u =>
            u.Provider == provider && u.ProviderUserId == providerUserId);

    public async Task<bool> ExistsByEmailAsync(string email) =>
        await _db.Users.AnyAsync(u => u.Email == email.ToLower());

    public async Task<bool> ExistsByUsernameAsync(string username) =>
        await _db.Users.AnyAsync(u => u.Username == username.ToLower());

    public async Task<IEnumerable<User>> FindAllByRoleAsync(UserRole role) =>
        await _db.Users.Where(u => u.Role == role).ToListAsync();

    public async Task<IEnumerable<User>> SearchByFullNameAsync(string query) =>
        await _db.Users
                .Where(u => EF.Functions.Like(u.FullName, $"%{query}%") ||
                            EF.Functions.Like(u.Username, $"%{query}%"))
                .Take(20)
                .ToListAsync();

    public async Task<User> CreateAsync(User user)
    {
        user.Email = user.Email.ToLower();
        user.Username = user.Username.ToLower();
        _db.Users.Add(user);
        await _db.SaveChangesAsync();
        return user;
    }

    public async Task<User> UpdateAsync(User user)
    {
        user.UpdatedAt = DateTime.UtcNow;
        _db.Users.Update(user);
        await _db.SaveChangesAsync();
        return user;
    }

    public async Task DeleteByUserIdAsync(Guid userId)
    {
        var user = await _db.Users
                            .IgnoreQueryFilters()
                            .FirstOrDefaultAsync(u => u.UserId == userId);
        if (user is not null)
        {
            _db.Users.Remove(user);
            await _db.SaveChangesAsync();
        }
    }

    // Platform Admin — bypasses the IsActive global query filter
    public async Task<User?> FindByUserIdIncludingInactiveAsync(Guid userId) =>
        await _db.Users
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(u => u.UserId == userId);

    public async Task<IEnumerable<User>> GetAllUsersAsync(int page, int pageSize) =>
        await _db.Users
                .IgnoreQueryFilters()
                .OrderBy(u => u.CreatedAt)
                 .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
}