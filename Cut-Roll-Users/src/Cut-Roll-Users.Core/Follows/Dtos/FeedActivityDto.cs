using Cut_Roll_Users.Core.Users.Dtos;
using Cut_Roll_Users.Core.Movies.Dtos;
using Cut_Roll_Users.Core.Reviews.Dtos;
using Cut_Roll_Users.Core.ListEntities.Dtos;

namespace Cut_Roll_Users.Core.Follows.Dtos;

public class FeedActivityDto
{
    public UserSimlified User { get; set; } = null!;
    public ActivityType Type { get; set; }
    public DateTime CreatedAt { get; set; }
    public MovieSimplifiedDto? Movie { get; set; }
    public ReviewSimplifiedDto? Review { get; set; }
    public ListEntitySimplifiedDto? List { get; set; }
}

public enum ActivityType
{
    MovieLike,
    MovieReview,
    MovieWatched,
    WantToWatch,
    ListCreated,
    ListLiked,
    ReviewLiked
}
