using Cut_Roll_Users.Core.Common.Dtos;
using Cut_Roll_Users.Core.Common.Repositories.Base;
using Cut_Roll_Users.Core.Comments.Dtos;
using Cut_Roll_Users.Core.Comments.Models;
using Cut_Roll_Users.Core.Comments.Repositories;
using Cut_Roll_Users.Core.Users.Dtos;
using Cut_Roll_Users.Infrastructure.Common.Data;
using Microsoft.EntityFrameworkCore;

namespace Cut_Roll_Users.Infrastructure.Comments.Repositories;

public class CommentEfCoreRepository : ICommentRepository
{
    private readonly UsersDbContext _context;

    public CommentEfCoreRepository(UsersDbContext context)
    {
        _context = context;
    }

    public async Task<Guid?> CreateAsync(CommentCreateDto entity)
    {
        var comment = new Comment
        {
            UserId = entity.UserId,
            ReviewId = entity.ReviewId,
            Content = entity.Content,
            CreatedAt = DateTime.UtcNow
        };

        _context.Comments.Add(comment);
        var result = await _context.SaveChangesAsync();
        
        return result > 0 ? comment.ReviewId : null;
    }

    public async Task<Guid?> UpdateAsync(CommentUpdateDto entity)
    {
        var comment = await _context.Comments
            .FirstOrDefaultAsync(c => c.UserId == entity.UserId && c.ReviewId == entity.ReviewId);
        
        if (comment == null) return null;

        comment.Content = entity.Content;
        
        var result = await _context.SaveChangesAsync();
        return result > 0 ? comment.ReviewId : null;
    }

    public async Task<Guid?> DeleteAsync(CommentDeleteDto entity)
    {
        var comment = await _context.Comments
            .FirstOrDefaultAsync(c => c.UserId == entity.UserId && c.ReviewId == entity.ReviewId);
        
        if (comment == null) return null;

        _context.Comments.Remove(comment);
        var result = await _context.SaveChangesAsync();
        
        return result > 0 ? comment.ReviewId : null;
    }

    public async Task<PagedResult<CommentResponseDto>> GetByReviewIdAsync(CommentPaginationReviewDto dto)
    {
        var query = _context.Comments
            .Include(c => c.User)
            .Where(c => c.ReviewId == dto.ReviewId)
            .OrderByDescending(c => c.CreatedAt);

        var totalCount = await query.CountAsync();

        var pageNumber = dto.Page <= 0 ? 1 : dto.Page;
        var pageSize = dto.PageSize <= 0 ? 10 : dto.PageSize;

        var comments = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(c => new CommentResponseDto
            {
                userSimlified = new UserSimlified
                {
                    Id = c.User.Id,
                    UserName = c.User.UserName,
                    Email = c.User.Email,
                    AvatarPath = c.User.AvatarPath
                },
                ReviewId = c.ReviewId,
                Content = c.Content,
                CreatedAt = c.CreatedAt
            })
            .ToListAsync();

        return new PagedResult<CommentResponseDto>
        {
            Data = comments,
            TotalCount = totalCount,
            Page = pageNumber,
            PageSize = pageSize
        };
    }

    public async Task<PagedResult<CommentResponseDto>> GetByUserIdAsync(CommentPaginationUserDto dto)
    {
        var query = _context.Comments
            .Include(c => c.User)
            .Where(c => c.UserId == dto.UserId)
            .OrderByDescending(c => c.CreatedAt);

        var totalCount = await query.CountAsync();

        var pageNumber = dto.Page <= 0 ? 1 : dto.Page;
        var pageSize = dto.PageSize <= 0 ? 10 : dto.PageSize;

        var comments = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(c => new CommentResponseDto
            {
                userSimlified = new UserSimlified
                {
                    Id = c.User.Id,
                    UserName = c.User.UserName,
                    Email = c.User.Email,
                    AvatarPath = c.User.AvatarPath
                },
                ReviewId = c.ReviewId,
                Content = c.Content,
                CreatedAt = c.CreatedAt
            })
            .ToListAsync();

        return new PagedResult<CommentResponseDto>
        {
            Data = comments,
            TotalCount = totalCount,
            Page = pageNumber,
            PageSize = pageSize
        };
    }

    public async Task<int> GetCommentCountByReviewIdAsync(Guid reviewId)
    {
        return await _context.Comments
            .Where(c => c.ReviewId == reviewId)
            .CountAsync();
    }
}
