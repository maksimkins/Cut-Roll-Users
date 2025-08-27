#pragma warning disable CS8618 

using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Cut_Roll_Users.Core.Common.Models.Base;
using Cut_Roll_Users.Core.Follows.Models;
using Cut_Roll_Users.Core.ListEntities.Models;
using Cut_Roll_Users.Core.ListLikes.Models;
using Cut_Roll_Users.Core.MovieLikes.Models;
using Cut_Roll_Users.Core.ReviewLikes.Models;
using Cut_Roll_Users.Core.Reviews.Models;
using Cut_Roll_Users.Core.WantToWatchFilms.Models;
using Cut_Roll_Users.Core.WatchedMovies.Models;

namespace Cut_Roll_Users.Core.Users.Models;

public class User : IBanable, IMuteable
{
    [Key]
    public required string Id { get; set; }
    public required string UserName { get; set; }
    public required string Email { get; set; }
    public string? AvatarPath { get; set; }

    [DefaultValue(false)]
    public bool IsBanned { get; set; }

    [DefaultValue(false)]
    public bool IsMuted { get; set; }
    public ICollection<Review> Reviews { get; set; } = [];
    public ICollection<ReviewLike> ReviewLikes { get; set; } = [];
    public ICollection<MovieLike> MovieLikes { get; set; } = [];
    public ICollection<WantToWatchFilm> WantToWatchFilms { get; set; } = [];
    public ICollection<WatchedMovie> Watched { get; set; } = [];
    public ICollection<ListEntity> Lists { get; set; } = [];
    public ICollection<ListLike> ListLikes { get; set; } = [];
    
    // Follow relationships
    public ICollection<Follow> Followers { get; set; } = [];
    public ICollection<Follow> Following { get; set; } = [];
}