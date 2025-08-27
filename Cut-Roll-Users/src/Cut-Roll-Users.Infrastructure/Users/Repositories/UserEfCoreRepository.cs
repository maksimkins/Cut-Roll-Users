
using Cut_Roll_Users.Core.Common.Dtos;
using Cut_Roll_Users.Core.Users.Dtos;
using Cut_Roll_Users.Core.Users.Models;
using Cut_Roll_Users.Core.Users.Repositories;
using Cut_Roll_Users.Infrastructure.Common.Data;
using Microsoft.EntityFrameworkCore;

namespace Cut_Roll_Users.Infrastructure.Users.Repositories;

public class UserEfCoreRepository : IUserRepository
{
    private readonly UsersDbContext _context;
    public UserEfCoreRepository(UsersDbContext context)
    {
        _context = context;
    }

    public async Task<string?> CreateAsync(UserCreateDto entity)
    {
        _context.Users.Add(new User
        {
            Id = entity.Id,
            UserName = entity.UserName,
            Email = entity.Email,
            AvatarPath = entity.AvatarPath,
        });

        var res = await _context.SaveChangesAsync();
        return res > 0 ? entity.Id : null;
    }

    public async Task<string?> DeleteByIdAsync(string id)
    {
        var user = await _context.Users.Where(u => u.Id == id).FirstOrDefaultAsync();
        if (user == null) return null;
        _context.Users.Remove(user);
        var res = await _context.SaveChangesAsync();

        return res > 0 ? user.Id : null;
    }

    public async Task<UserResponseDto?> GetByIdAsync(string id)
    {
        var user = await _context.Users.Where(u => u.Id == id).FirstOrDefaultAsync();
        if (user == null) return null;
        
        return new UserResponseDto
        {
            Id = user.Id,
            Username = user.UserName,
            Email = user.Email,
            AvatarPath = user.AvatarPath,
            IsBanned = user.IsBanned,
            IsMuted = user.IsMuted,
        };
    }

    public async Task<double> GetUserAverageRatingAsync(string userId)
    {
    var ratings = await _context.Reviews
        .Where(r => r.UserId == userId)
        .Select(r => (double?)r.Rating) 
        .ToListAsync();

    if (ratings.Count == 0)
        return 0.0; 

    return ratings.Average() ?? 0.0;
    }

    public async Task<UserResponseDto?> GetUserByEmailAsync(string email)
    {
        var user = await _context.Users.Where(u => u.Email == email).FirstOrDefaultAsync();
        if (user == null) return null;

        return new UserResponseDto
        {
            Id = user.Id,
            Username = user.UserName,
            Email = user.Email,
            AvatarPath = user.AvatarPath,
            IsBanned = user.IsBanned,
            IsMuted = user.IsMuted,
        };
    }

    public async Task<UserResponseDto?> GetUserByUsernameAsync(string username)
    {
        var user = await _context.Users.Where(u => u.UserName == username).FirstOrDefaultAsync();
        if (user == null) return null;

        return new UserResponseDto
        {
            Id = user.Id,
            Username = user.UserName,
            Email = user.Email,
            AvatarPath = user.AvatarPath,
            IsBanned = user.IsBanned,
            IsMuted = user.IsMuted,
        };
    }

    public async Task<int> GetUserListCountAsync(string userId)
    {
        return await _context.ListEntities
            .Where(l => l.UserId == userId)
            .CountAsync();
    }

    public async Task<int> GetUserMovieLikeCountAsync(string userId)
    {
        return await _context.MovieLikes
            .Where(ml => ml.UserId == userId)
            .CountAsync();
    }

    public async Task<int> GetUserReviewCountAsync(string userId)
    {
        return await _context.Reviews
            .Where(r => r.UserId == userId)
            .CountAsync();
    }

    public async Task<IQueryable<User>> GetUsersAsQueryableAsync()
    {
        return await Task.FromResult(_context.Users.AsNoTracking().AsQueryable()); 
    }

    public async Task<int> GetUserWantToWatchCountAsync(string userId)
    {
        return await _context.WantToWatchMovies
            .Where(w => w.UserId == userId)
            .CountAsync();
    }
    
    public async Task<int> GetUserWatchedCountAsync(string userId)
    {
        return await _context.WatchedMovies
            .Where(w => w.UserId == userId)
            .CountAsync();
    }

    public async Task<PagedResult<UserResponseDto>> SearchUsersAsync(UserSearchDto dto)
    {
        var query = _context.Users.AsQueryable();

    
        if (!string.IsNullOrWhiteSpace(dto.SearchTerm))
        {
            var term = $"%{dto.SearchTerm}%"; 
            query = query.Where(u =>
                EF.Functions.ILike(u.UserName, term) ||
                EF.Functions.ILike(u.Email, term));
        }

        
        if (dto.IsBanned.HasValue)
        {
            query = query.Where(u => u.IsBanned == dto.IsBanned.Value);
        }

        if (dto.IsMuted.HasValue)
        {
            query = query.Where(u => u.IsMuted == dto.IsMuted.Value);
        }
        
        var totalCount = await query.CountAsync();

        
        var pageNumber = dto.PageNumber <= 0 ? 1 : dto.PageNumber;
        var pageSize = dto.PageSize <= 0 ? 10 : dto.PageSize;

        var users = await query
            .OrderBy(u => u.UserName) 
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(u => new UserResponseDto
            {
                Id = u.Id,
                Username = u.UserName,
                Email = u.Email,
                IsBanned = u.IsBanned,
                IsMuted = u.IsMuted,
            })
            .ToListAsync();

        
        return new PagedResult<UserResponseDto>
        {
            Data = users,
            TotalCount = totalCount,
            Page = pageNumber,
            PageSize = pageSize
        };
    }

    public async Task<string?> UpdateAsync(UserUpdateDto dto)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == dto.Id);
        if (user == null) return null;

        user.UserName = dto.UserName ?? user.UserName;
        user.Email = dto.Email ?? user.Email;

        var res = await _context.SaveChangesAsync();
        return res > 0 ? user.Id : null;
    }

    public async Task<string?> UpdateAvatarAsync(UserUpdateAvatarDto dto)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == dto.Id);
        if (user == null) return null;
        user.AvatarPath = dto.AvatarPath;
        var res = await _context.SaveChangesAsync();
        return res > 0 ? user.Id : null;
    }

    public async Task<bool> UserExistsByEmailAsync(string email)
    {
        return await _context.Users.AnyAsync(u => u.UserName == email);
    }

    public async Task<bool> UserExistsByUsernameAsync(string username)
    {
        return await _context.Users.AnyAsync(u => u.UserName == username);
    }
}
