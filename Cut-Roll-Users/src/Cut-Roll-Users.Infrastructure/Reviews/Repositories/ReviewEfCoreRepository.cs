using Cut_Roll_Users.Core.Common.Dtos;
using Cut_Roll_Users.Core.Common.Repositories.Base;
using Cut_Roll_Users.Core.Reviews.Dtos;
using Cut_Roll_Users.Core.Reviews.Models;
using Cut_Roll_Users.Core.Reviews.Repositories;
using Cut_Roll_Users.Core.Users.Dtos;
using Cut_Roll_Users.Infrastructure.Common.Data;
using Microsoft.EntityFrameworkCore;

namespace Cut_Roll_Users.Infrastructure.Reviews.Repositories;

public class ReviewEfCoreRepository : IReviewRepository
{
    private readonly UsersDbContext _context;

    public ReviewEfCoreRepository(UsersDbContext context)
    {
        _context = context;
    }

    public async Task<Guid?> CreateAsync(ReviewCreateDto entity)
    {
        var review = new Review
        {
            UserId = entity.UserId,
            MovieId = entity.MovieId,
            Content = entity.Content,
            Rating = entity.Rating,
            CreatedAt = DateTime.UtcNow
        };

        _context.Reviews.Add(review);
        var result = await _context.SaveChangesAsync();

        if (result > 0)
        {
            var movie = await _context.Movies
                .Include(m => m.Reviews)
                .FirstOrDefaultAsync(m => m.Id == entity.MovieId);

            if (movie != null && movie.Reviews.Any())
            {
                movie.RatingAverage = movie.Reviews.Average(r => r.Rating);
                await _context.SaveChangesAsync();
            }

            return review.Id;
        }

        return null;
    }

    public async Task<ReviewResponseDto?> GetByIdAsync(Guid id)
    {
        var review = await _context.Reviews
            .Include(r => r.User)
            .Include(r => r.Likes)
            .Include(r => r.Comments)
            .FirstOrDefaultAsync(r => r.Id == id);

        if (review == null) return null;

        return new ReviewResponseDto
        {
            Id = review.Id,
            UserSimplified = new UserSimplified
            {
                Id = review.User.Id,
                UserName = review.User.UserName,
                Email = review.User.Email,
                AvatarPath = review.User.AvatarPath
            },
            MovieId = review.MovieId,
            Content = review.Content,
            Rating = (int)review.Rating,
            CreatedAt = review.CreatedAt,
            LikesCount = review.Likes.Count,
            CommentsCount = review.Comments.Count
        };
    }

    public async Task<Guid?> UpdateAsync(ReviewUpdateDto entity)
    {
        var review = await _context.Reviews
            .FirstOrDefaultAsync(r => r.Id == entity.Id);

        if (review == null) return null;

        if (entity.Content != null)
            review.Content = entity.Content;

        if (entity.Rating.HasValue)
            review.Rating = entity.Rating.Value;

        var result = await _context.SaveChangesAsync();
        return result > 0 ? review.Id : null;
    }

    public async Task<Guid?> DeleteByIdAsync(Guid id)
    {
        var review = await _context.Reviews
            .FirstOrDefaultAsync(r => r.Id == id);

        if (review == null) return null;

        _context.Reviews.Remove(review);
        var result = await _context.SaveChangesAsync();

        return result > 0 ? review.Id : null;
    }

    public async Task<ReviewResponseDto?> GetByUserAndMovieAsync(string userId, Guid movieId)
    {
        var review = await _context.Reviews
            .Include(r => r.User)
            .Include(r => r.Likes)
            .Include(r => r.Comments)
            .FirstOrDefaultAsync(r => r.UserId == userId && r.MovieId == movieId);

        if (review == null) return null;

        return new ReviewResponseDto
        {
            Id = review.Id,
            UserSimplified = new UserSimplified
            {
                Id = review.User.Id,
                UserName = review.User.UserName,
                Email = review.User.Email,
                AvatarPath = review.User.AvatarPath
            },
            MovieId = review.MovieId,
            Content = review.Content,
            Rating = (int)review.Rating,
            CreatedAt = review.CreatedAt,
            LikesCount = review.Likes.Count,
            CommentsCount = review.Comments.Count
        };
    }

    public async Task<PagedResult<ReviewResponseDto>> GetByMovieIdAsync(ReviewPaginationMovieDto dto)
    {
        var query = _context.Reviews
            .Include(r => r.User)
            .Include(r => r.Likes)
            .Include(r => r.Comments)
            .Where(r => r.MovieId == dto.MovieId)
            .OrderByDescending(r => r.CreatedAt);

        var totalCount = await query.CountAsync();

        var pageNumber = dto.Page <= 0 ? 1 : dto.Page;
        var pageSize = dto.PageSize <= 0 ? 10 : dto.PageSize;

        var reviews = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(r => new ReviewResponseDto
            {
                Id = r.Id,
                UserSimplified = new UserSimplified
                {
                    Id = r.User.Id,
                    UserName = r.User.UserName,
                    Email = r.User.Email,
                    AvatarPath = r.User.AvatarPath
                },
                MovieId = r.MovieId,
                Content = r.Content,
                Rating = (int)r.Rating,
                CreatedAt = r.CreatedAt,
                LikesCount = r.Likes.Count,
                CommentsCount = r.Comments.Count
            })
            .ToListAsync();

        return new PagedResult<ReviewResponseDto>
        {
            Data = reviews,
            TotalCount = totalCount,
            Page = pageNumber,
            PageSize = pageSize
        };
    }

    public async Task<PagedResult<ReviewResponseDto>> GetByUserIdAsync(ReviewPaginationUserDto dto)
    {
        var query = _context.Reviews
            .Include(r => r.User)
            .Include(r => r.Likes)
            .Include(r => r.Comments)
            .Where(r => r.UserId == dto.UserId.ToString())
            .OrderByDescending(r => r.CreatedAt);

        var totalCount = await query.CountAsync();

        var pageNumber = dto.Page <= 0 ? 1 : dto.Page;
        var pageSize = dto.PageSize <= 0 ? 10 : dto.PageSize;

        var reviews = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(r => new ReviewResponseDto
            {
                Id = r.Id,
                UserSimplified = new UserSimplified
                {
                    Id = r.User.Id,
                    UserName = r.User.UserName,
                    Email = r.User.Email,
                    AvatarPath = r.User.AvatarPath
                },
                MovieId = r.MovieId,
                Content = r.Content,
                Rating = (int)r.Rating,
                CreatedAt = r.CreatedAt,
                LikesCount = r.Likes.Count,
                CommentsCount = r.Comments.Count
            })
            .ToListAsync();

        return new PagedResult<ReviewResponseDto>
        {
            Data = reviews,
            TotalCount = totalCount,
            Page = pageNumber,
            PageSize = pageSize
        };
    }

    public async Task<double> GetAverageRatingByMovieIdAsync(Guid movieId)
    {
        return await _context.Reviews
            .Where(r => r.MovieId == movieId)
            .Select(r => (double?)r.Rating)
            .AverageAsync() ?? 0.0;
    }

    public async Task<int> GetReviewCountByMovieIdAsync(Guid movieId)
    {
        return await _context.Reviews
            .Where(r => r.MovieId == movieId)
            .CountAsync();
    }

    public async Task<bool> IsReviewOwnedByUserAsync(Guid reviewId, string userId)
    {
        var review = await _context.Reviews.Where(r => r.Id == reviewId).FirstOrDefaultAsync();
        return review?.UserId == userId;
    }

    public async Task<PagedResult<ReviewResponseDto>> SearchAsync(ReviewSearchDto request)
    {
        var query = _context.Reviews
            .Include(r => r.User)
            .Include(r => r.Movie)
            .Include(r => r.Likes)
            .Include(r => r.Comments)
            .AsQueryable();


        if (!string.IsNullOrWhiteSpace(request.UserId))
        {
            query = query.Where(r => r.UserId == request.UserId);
        }

        if (request.MovieId != null && request.MovieId == Guid.Empty)
        {
            query = query.Where(r => r.MovieId == request.MovieId);
        }

        if (request.MinRating.HasValue)
        {
            query = query.Where(r => r.Rating >= request.MinRating.Value);
        }

        if (request.MaxRating.HasValue)
        {
            query = query.Where(r => r.Rating <= request.MaxRating.Value);
        }

        if (request.CreatedAfter.HasValue)
        {
            query = query.Where(r => r.CreatedAt >= request.CreatedAfter.Value);
        }

        if (request.CreatedBefore.HasValue)
        {
            query = query.Where(r => r.CreatedAt <= request.CreatedBefore.Value);
        }


        query = request.SortBy switch
        {
            ReviewSortBy.Rating => request.SortDescending
                ? query.OrderByDescending(r => r.Rating)
                : query.OrderBy(r => r.Rating),

            ReviewSortBy.Likes => request.SortDescending
                ? query.OrderByDescending(r => r.Likes.Count)
                : query.OrderBy(r => r.Likes.Count),

            ReviewSortBy.CreatedAt or _ => request.SortDescending
                ? query.OrderByDescending(r => r.CreatedAt)
                : query.OrderBy(r => r.CreatedAt),
        };

        var totalCount = await query.CountAsync();
        var page = request.Page <= 0 ? 1 : request.Page;
        var pageSize = request.PageSize <= 0 ? 10 : request.PageSize;

        var reviews = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(r => new ReviewResponseDto
            {
                Id = r.Id,
                UserSimplified = new UserSimplified
                {
                    Id = r.User.Id,
                    UserName = r.User.UserName,
                    Email = r.User.Email,
                    AvatarPath = r.User.AvatarPath
                },
                MovieId = r.MovieId,
                Content = r.Content,
                Rating = r.Rating, 
                CreatedAt = r.CreatedAt,
                LikesCount = r.Likes.Count,
                CommentsCount = r.Comments.Count
            }).ToListAsync();

        return new PagedResult<ReviewResponseDto>
        {
            Data = reviews,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }
        
}
