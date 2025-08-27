
using Cut_Roll_Users.Core.Common.Dtos;
using Cut_Roll_Users.Core.Users.Dtos;
using Cut_Roll_Users.Core.Users.Models;
using Cut_Roll_Users.Core.Users.Repositories;
using Cut_Roll_Users.Core.Users.Services;

namespace Cut_Roll_Users.Infrastructure.Users.Services;

public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;

    public UserService(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<string> CreateUserAsync(UserCreateDto? userCreateDto)
    {
        if (userCreateDto == null)
            throw new ArgumentNullException(nameof(userCreateDto), "User creation data cannot be null.");
        if (string.IsNullOrWhiteSpace(userCreateDto.UserName) || string.IsNullOrWhiteSpace(userCreateDto.Email)
         || string.IsNullOrWhiteSpace(userCreateDto.RoleId) || string.IsNullOrWhiteSpace(userCreateDto.Id))
            throw new ArgumentNullException($"{nameof(userCreateDto.UserName)}, {nameof(userCreateDto.Email)}, {nameof(userCreateDto.RoleId)}, {nameof(userCreateDto.Id)}");

        return await _userRepository.CreateAsync(userCreateDto) ??
            throw new InvalidOperationException("Failed to create user.");
    }



    public async Task<string> DeleteUserByIdAsync(string? userId)
    {
        if (string.IsNullOrWhiteSpace(userId))
            throw new ArgumentNullException(nameof(userId), "User ID cannot be null or empty.");
            
        return await _userRepository.DeleteByIdAsync(userId) ??
            throw new InvalidOperationException("Failed to delete user.");
    }

    public Task<double> GetUserAverageRatingAsync(string? userId)
    {
        if (string.IsNullOrWhiteSpace(userId))
            throw new ArgumentNullException(nameof(userId), "UserId cannot be null or empty.");
            
        return _userRepository.GetUserAverageRatingAsync(userId);
    }

    public async Task<UserResponseDto?> GetUserByEmailAsync(string? email)
    {
        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentNullException(nameof(email), "Email cannot be null or empty.");

        return await _userRepository.GetUserByEmailAsync(email);
    }

    public async Task<UserResponseDto?> GetUserByIdAsync(string? userId)
    {
        if (string.IsNullOrWhiteSpace(userId))
            throw new ArgumentNullException(nameof(userId), "User ID cannot be null or empty.");

        return await _userRepository.GetByIdAsync(userId);
    }

    public async Task<UserResponseDto?> GetUserByUsernameAsync(string? username)
    {
        if (string.IsNullOrWhiteSpace(username))
            throw new ArgumentNullException(nameof(username), "Username cannot be null or empty.");

        return await _userRepository.GetUserByUsernameAsync(username);
    }

    public Task<int> GetUserListCountAsync(string? userId)
    {
        if (string.IsNullOrWhiteSpace(userId))
            throw new ArgumentNullException(nameof(userId), "UserId cannot be null or empty.");
            
        return _userRepository.GetUserListCountAsync(userId);
    }

    public Task<int> GetUserMovieLikeCountAsync(string? userId)
    {
        if (string.IsNullOrWhiteSpace(userId))
            throw new ArgumentNullException(nameof(userId), "UserId cannot be null or empty.");
            
        return _userRepository.GetUserReviewCountAsync(userId);
    }

    public Task<int> GetUserReviewCountAsync(string? userId)
    {
        if (string.IsNullOrWhiteSpace(userId))
            throw new ArgumentNullException(nameof(userId), "UserId cannot be null or empty.");
            
        return _userRepository.GetUserReviewCountAsync(userId);
    }

    public async Task<IQueryable<User>> GetUsersAsQueryableAsync()
    {
        return await _userRepository.GetUsersAsQueryableAsync();
    }

    public Task<int> GetUserWantToWatchCountAsync(string? userId)
    {
        if (string.IsNullOrWhiteSpace(userId))
            throw new ArgumentNullException(nameof(userId), "UserId cannot be null or empty.");
            
        return _userRepository.GetUserWantToWatchCountAsync(userId);
    }

    public Task<int> GetUserWatchedCountAsync(string? userId)
    {
        if (string.IsNullOrWhiteSpace(userId))
            throw new ArgumentNullException(nameof(userId), "UserId cannot be null or empty.");
            
        return _userRepository.GetUserWatchedCountAsync(userId);
    }

    public async Task<PagedResult<UserResponseDto>> SearchUsersAsync(UserSearchDto? dto)
    {
        if (dto == null)
            throw new ArgumentNullException(nameof(dto), "User search data cannot be null.");

        return await _userRepository.SearchUsersAsync(dto);   
    }

    public async Task<string> UpdateUserAsync(UserUpdateDto? userUpdateDto)
    {
        if (userUpdateDto == null)
            throw new ArgumentNullException(nameof(userUpdateDto), "User update data cannot be null.");
        if (string.IsNullOrWhiteSpace(userUpdateDto.Id))
            throw new ArgumentNullException(nameof(userUpdateDto.Id), "User ID cannot be null or empty.");
        if (string.IsNullOrWhiteSpace(userUpdateDto.UserName) && string.IsNullOrWhiteSpace(userUpdateDto.Email))
            throw new ArgumentNullException($"{nameof(userUpdateDto.UserName)}, {nameof(userUpdateDto.Email)}");

        return await _userRepository.UpdateAsync(userUpdateDto) ??
            throw new InvalidOperationException("Failed to update user.");
        
    }

 

    public async Task<string> UpdateUserAvatarAsync(UserUpdateAvatarDto? dto)
    {
        if (dto == null)
            throw new ArgumentNullException(nameof(dto), "User avatar update data cannot be null.");
        if (string.IsNullOrWhiteSpace(dto.Id) || string.IsNullOrWhiteSpace(dto.AvatarPath))
            throw new ArgumentNullException($"{nameof(dto.Id)}, {nameof(dto.AvatarPath)}");

        var user = await _userRepository.GetByIdAsync(dto.Id) ??
            throw new ArgumentException("User not found.");
            
        return await _userRepository.UpdateAvatarAsync(dto) ??
            throw new InvalidOperationException("Failed to update user avatar.");
    }


    public Task<bool> UserExistsByEmailAsync(string? email)
    {
        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentNullException(nameof(email), "Email cannot be null or empty.");
        
        return _userRepository.UserExistsByEmailAsync(email);
    }

    public async Task<bool> UserExistsByUsernameAsync(string? username)
    {
        if (string.IsNullOrWhiteSpace(username))
            throw new ArgumentNullException(nameof(username), "Username cannot be null or empty.");

        return await _userRepository.UserExistsByUsernameAsync(username);
    }


}
