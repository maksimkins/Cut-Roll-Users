using Cut_Roll_Users.Core.Common.Dtos;
using Cut_Roll_Users.Core.Common.Repositories.Base;
using Cut_Roll_Users.Core.Users.Dtos;
using Cut_Roll_Users.Core.Users.Models;

namespace Cut_Roll_Users.Core.Users.Repositories;

public interface IUserRepository :
    ICreateAsync<UserCreateDto, string?>,
    IGetByIdAsync<string, UserResponseDto?>,
    IUpdateAsync<UserUpdateDto, string?>,
    IDeleteByIdAsync<string, string?>
{
    Task<UserResponseDto?> GetUserByUsernameAsync(string username);
    Task<UserResponseDto?> GetUserByEmailAsync(string email);
    Task<PagedResult<UserResponseDto>> SearchUsersAsync(UserSearchDto dto);
    Task<bool> UserExistsByUsernameAsync(string username);
    Task<bool> UserExistsByEmailAsync(string email);
    Task<string?> UpdateAvatarAsync(UserUpdateAvatarDto dto);
    Task<IQueryable<User>> GetUsersAsQueryableAsync();
    
    // User activity methods
    Task<int> GetUserReviewCountAsync(string userId);
    Task<int> GetUserWatchedCountAsync(string userId);
    Task<int> GetUserMovieLikeCountAsync(string userId);
    Task<int> GetUserWantToWatchCountAsync(string userId);
    Task<int> GetUserListCountAsync(string userId);
    Task<double> GetUserAverageRatingAsync(string userId);
}
