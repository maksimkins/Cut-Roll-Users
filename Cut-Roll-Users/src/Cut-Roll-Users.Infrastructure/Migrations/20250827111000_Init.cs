using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Cut_Roll_Users.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Init : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("CREATE EXTENSION IF NOT EXISTS \"pgcrypto\";");
            migrationBuilder.CreateTable(
                name: "countries",
                columns: table => new
                {
                    Iso3166_1 = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_countries", x => x.Iso3166_1);
                });

            migrationBuilder.CreateTable(
                name: "ExecutedScripts",
                columns: table => new
                {
                    ScriptName = table.Column<string>(type: "text", nullable: false),
                    ExecutedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExecutedScripts", x => x.ScriptName);
                });

            migrationBuilder.CreateTable(
                name: "genres",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    Name = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_genres", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "keywords",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    Name = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_keywords", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "movies",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    Title = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    Tagline = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Overview = table.Column<string>(type: "character varying(5000)", maxLength: 5000, nullable: false),
                    ReleaseDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Runtime = table.Column<int>(type: "integer", nullable: true, defaultValue: 0),
                    VoteAverage = table.Column<float>(type: "real", nullable: true, defaultValue: 0f),
                    Budget = table.Column<long>(type: "bigint", nullable: true, defaultValue: 0L),
                    Revenue = table.Column<long>(type: "bigint", nullable: true, defaultValue: 0L),
                    Homepage = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    ImdbId = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    Rating = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    RatingAverage = table.Column<float>(type: "real", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_movies", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "people",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    Name = table.Column<string>(type: "text", nullable: false),
                    ProfilePath = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_people", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "spoken_languages",
                columns: table => new
                {
                    Iso639_1 = table.Column<string>(type: "text", nullable: false),
                    EnglishName = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_spoken_languages", x => x.Iso639_1);
                });

            migrationBuilder.CreateTable(
                name: "users",
                columns: table => new
                {
                    Id = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: false),
                    UserName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Email = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    AvatarPath = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    IsBanned = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IsMuted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "production_companies",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    Name = table.Column<string>(type: "text", nullable: false),
                    CountryCode = table.Column<string>(type: "text", nullable: true),
                    LogoPath = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_production_companies", x => x.Id);
                    table.ForeignKey(
                        name: "FK_production_companies_countries_CountryCode",
                        column: x => x.CountryCode,
                        principalTable: "countries",
                        principalColumn: "Iso3166_1");
                });

            migrationBuilder.CreateTable(
                name: "movie_genres",
                columns: table => new
                {
                    MovieId = table.Column<Guid>(type: "uuid", nullable: false),
                    GenreId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_movie_genres", x => new { x.GenreId, x.MovieId });
                    table.ForeignKey(
                        name: "FK_movie_genres_genres_GenreId",
                        column: x => x.GenreId,
                        principalTable: "genres",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_movie_genres_movies_MovieId",
                        column: x => x.MovieId,
                        principalTable: "movies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "movie_images",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    MovieId = table.Column<Guid>(type: "uuid", nullable: false),
                    Type = table.Column<string>(type: "text", nullable: false),
                    FilePath = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_movie_images", x => x.Id);
                    table.ForeignKey(
                        name: "FK_movie_images_movies_MovieId",
                        column: x => x.MovieId,
                        principalTable: "movies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "movie_keywords",
                columns: table => new
                {
                    MovieId = table.Column<Guid>(type: "uuid", nullable: false),
                    KeywordId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_movie_keywords", x => new { x.MovieId, x.KeywordId });
                    table.ForeignKey(
                        name: "FK_movie_keywords_keywords_KeywordId",
                        column: x => x.KeywordId,
                        principalTable: "keywords",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_movie_keywords_movies_MovieId",
                        column: x => x.MovieId,
                        principalTable: "movies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "movie_origin_countries",
                columns: table => new
                {
                    MovieId = table.Column<Guid>(type: "uuid", nullable: false),
                    CountryCode = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_movie_origin_countries", x => new { x.MovieId, x.CountryCode });
                    table.ForeignKey(
                        name: "FK_movie_origin_countries_countries_CountryCode",
                        column: x => x.CountryCode,
                        principalTable: "countries",
                        principalColumn: "Iso3166_1",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_movie_origin_countries_movies_MovieId",
                        column: x => x.MovieId,
                        principalTable: "movies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "movie_production_countries",
                columns: table => new
                {
                    MovieId = table.Column<Guid>(type: "uuid", nullable: false),
                    CountryCode = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_movie_production_countries", x => new { x.MovieId, x.CountryCode });
                    table.ForeignKey(
                        name: "FK_movie_production_countries_countries_CountryCode",
                        column: x => x.CountryCode,
                        principalTable: "countries",
                        principalColumn: "Iso3166_1",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_movie_production_countries_movies_MovieId",
                        column: x => x.MovieId,
                        principalTable: "movies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "movie_videos",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    MovieId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Type = table.Column<string>(type: "text", nullable: true),
                    Site = table.Column<string>(type: "text", nullable: false),
                    Key = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_movie_videos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_movie_videos_movies_MovieId",
                        column: x => x.MovieId,
                        principalTable: "movies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "cast",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    MovieId = table.Column<Guid>(type: "uuid", nullable: false),
                    PersonId = table.Column<Guid>(type: "uuid", nullable: false),
                    Character = table.Column<string>(type: "text", nullable: true),
                    CastOrder = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_cast", x => x.Id);
                    table.ForeignKey(
                        name: "FK_cast_movies_MovieId",
                        column: x => x.MovieId,
                        principalTable: "movies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_cast_people_PersonId",
                        column: x => x.PersonId,
                        principalTable: "people",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "crew",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    MovieId = table.Column<Guid>(type: "uuid", nullable: false),
                    PersonId = table.Column<Guid>(type: "uuid", nullable: false),
                    Job = table.Column<string>(type: "text", nullable: true),
                    Department = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_crew", x => x.Id);
                    table.ForeignKey(
                        name: "FK_crew_movies_MovieId",
                        column: x => x.MovieId,
                        principalTable: "movies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_crew_people_PersonId",
                        column: x => x.PersonId,
                        principalTable: "people",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "movie_spoken_languages",
                columns: table => new
                {
                    MovieId = table.Column<Guid>(type: "uuid", nullable: false),
                    LanguageCode = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_movie_spoken_languages", x => new { x.MovieId, x.LanguageCode });
                    table.ForeignKey(
                        name: "FK_movie_spoken_languages_movies_MovieId",
                        column: x => x.MovieId,
                        principalTable: "movies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_movie_spoken_languages_spoken_languages_LanguageCode",
                        column: x => x.LanguageCode,
                        principalTable: "spoken_languages",
                        principalColumn: "Iso639_1",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "follows",
                columns: table => new
                {
                    FollowerId = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: false),
                    FollowingId = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_follows", x => new { x.FollowerId, x.FollowingId });
                    table.CheckConstraint("CK_Follow_SelfFollow", "followerid != followingid");
                    table.ForeignKey(
                        name: "FK_follows_users_FollowerId",
                        column: x => x.FollowerId,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_follows_users_FollowingId",
                        column: x => x.FollowingId,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "list_entities",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    UserId = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: false),
                    Title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_list_entities", x => x.Id);
                    table.ForeignKey(
                        name: "FK_list_entities_users_UserId",
                        column: x => x.UserId,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "movie_likes",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: false),
                    MovieId = table.Column<Guid>(type: "uuid", nullable: false),
                    LikedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_movie_likes", x => new { x.UserId, x.MovieId });
                    table.ForeignKey(
                        name: "FK_movie_likes_movies_MovieId",
                        column: x => x.MovieId,
                        principalTable: "movies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_movie_likes_users_UserId",
                        column: x => x.UserId,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "reviews",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    UserId = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: false),
                    MovieId = table.Column<Guid>(type: "uuid", nullable: false),
                    Content = table.Column<string>(type: "character varying(5000)", maxLength: 5000, nullable: false),
                    Rating = table.Column<float>(type: "real", nullable: false, defaultValue: 0f),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_reviews", x => x.Id);
                    table.ForeignKey(
                        name: "FK_reviews_movies_MovieId",
                        column: x => x.MovieId,
                        principalTable: "movies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_reviews_users_UserId",
                        column: x => x.UserId,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "want_to_watch_films",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: false),
                    MovieId = table.Column<Guid>(type: "uuid", nullable: false),
                    AddedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_want_to_watch_films", x => new { x.UserId, x.MovieId });
                    table.ForeignKey(
                        name: "FK_want_to_watch_films_movies_MovieId",
                        column: x => x.MovieId,
                        principalTable: "movies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_want_to_watch_films_users_UserId",
                        column: x => x.UserId,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "watched_movies",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: false),
                    MovieId = table.Column<Guid>(type: "uuid", nullable: false),
                    WatchedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_watched_movies", x => new { x.UserId, x.MovieId });
                    table.ForeignKey(
                        name: "FK_watched_movies_movies_MovieId",
                        column: x => x.MovieId,
                        principalTable: "movies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_watched_movies_users_UserId",
                        column: x => x.UserId,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "movie_production_companies",
                columns: table => new
                {
                    MovieId = table.Column<Guid>(type: "uuid", nullable: false),
                    CompanyId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_movie_production_companies", x => new { x.MovieId, x.CompanyId });
                    table.ForeignKey(
                        name: "FK_movie_production_companies_movies_MovieId",
                        column: x => x.MovieId,
                        principalTable: "movies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_movie_production_companies_production_companies_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "production_companies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "list_likes",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: false),
                    ListId = table.Column<Guid>(type: "uuid", nullable: false),
                    LikedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_list_likes", x => new { x.UserId, x.ListId });
                    table.ForeignKey(
                        name: "FK_list_likes_list_entities_ListId",
                        column: x => x.ListId,
                        principalTable: "list_entities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_list_likes_users_UserId",
                        column: x => x.UserId,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "list_movies",
                columns: table => new
                {
                    ListId = table.Column<Guid>(type: "uuid", nullable: false),
                    MovieId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_list_movies", x => new { x.ListId, x.MovieId });
                    table.ForeignKey(
                        name: "FK_list_movies_list_entities_ListId",
                        column: x => x.ListId,
                        principalTable: "list_entities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_list_movies_movies_MovieId",
                        column: x => x.MovieId,
                        principalTable: "movies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "comments",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: false),
                    ReviewId = table.Column<Guid>(type: "uuid", nullable: false),
                    Content = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_comments", x => new { x.UserId, x.ReviewId });
                    table.ForeignKey(
                        name: "FK_comments_reviews_ReviewId",
                        column: x => x.ReviewId,
                        principalTable: "reviews",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_comments_users_UserId",
                        column: x => x.UserId,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "review_likes",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: false),
                    ReviewId = table.Column<Guid>(type: "uuid", nullable: false),
                    LikedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_review_likes", x => new { x.UserId, x.ReviewId });
                    table.ForeignKey(
                        name: "FK_review_likes_reviews_ReviewId",
                        column: x => x.ReviewId,
                        principalTable: "reviews",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_review_likes_users_UserId",
                        column: x => x.UserId,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_cast_MovieId",
                table: "cast",
                column: "MovieId");

            migrationBuilder.CreateIndex(
                name: "IX_cast_PersonId",
                table: "cast",
                column: "PersonId");

            migrationBuilder.CreateIndex(
                name: "IX_comments_ReviewId",
                table: "comments",
                column: "ReviewId");

            migrationBuilder.CreateIndex(
                name: "IX_crew_MovieId",
                table: "crew",
                column: "MovieId");

            migrationBuilder.CreateIndex(
                name: "IX_crew_PersonId",
                table: "crew",
                column: "PersonId");

            migrationBuilder.CreateIndex(
                name: "IX_follows_CreatedAt",
                table: "follows",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_follows_FollowerId",
                table: "follows",
                column: "FollowerId");

            migrationBuilder.CreateIndex(
                name: "IX_follows_FollowingId",
                table: "follows",
                column: "FollowingId");

            migrationBuilder.CreateIndex(
                name: "IX_list_entities_UserId",
                table: "list_entities",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_list_likes_ListId",
                table: "list_likes",
                column: "ListId");

            migrationBuilder.CreateIndex(
                name: "IX_list_movies_MovieId",
                table: "list_movies",
                column: "MovieId");

            migrationBuilder.CreateIndex(
                name: "IX_movie_genres_MovieId",
                table: "movie_genres",
                column: "MovieId");

            migrationBuilder.CreateIndex(
                name: "IX_movie_images_MovieId",
                table: "movie_images",
                column: "MovieId");

            migrationBuilder.CreateIndex(
                name: "IX_movie_keywords_KeywordId",
                table: "movie_keywords",
                column: "KeywordId");

            migrationBuilder.CreateIndex(
                name: "IX_movie_likes_MovieId",
                table: "movie_likes",
                column: "MovieId");

            migrationBuilder.CreateIndex(
                name: "IX_movie_origin_countries_CountryCode",
                table: "movie_origin_countries",
                column: "CountryCode");

            migrationBuilder.CreateIndex(
                name: "IX_movie_production_companies_CompanyId",
                table: "movie_production_companies",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_movie_production_countries_CountryCode",
                table: "movie_production_countries",
                column: "CountryCode");

            migrationBuilder.CreateIndex(
                name: "IX_movie_spoken_languages_LanguageCode",
                table: "movie_spoken_languages",
                column: "LanguageCode");

            migrationBuilder.CreateIndex(
                name: "IX_movie_videos_MovieId",
                table: "movie_videos",
                column: "MovieId");

            migrationBuilder.CreateIndex(
                name: "IX_production_companies_CountryCode",
                table: "production_companies",
                column: "CountryCode");

            migrationBuilder.CreateIndex(
                name: "IX_review_likes_ReviewId",
                table: "review_likes",
                column: "ReviewId");

            migrationBuilder.CreateIndex(
                name: "IX_reviews_MovieId",
                table: "reviews",
                column: "MovieId");

            migrationBuilder.CreateIndex(
                name: "IX_reviews_UserId_MovieId",
                table: "reviews",
                columns: new[] { "UserId", "MovieId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_users_Email",
                table: "users",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_users_UserName",
                table: "users",
                column: "UserName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_want_to_watch_films_MovieId",
                table: "want_to_watch_films",
                column: "MovieId");

            migrationBuilder.CreateIndex(
                name: "IX_watched_movies_MovieId",
                table: "watched_movies",
                column: "MovieId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP EXTENSION IF EXISTS \"pgcrypto\";");
            migrationBuilder.DropTable(
                name: "cast");

            migrationBuilder.DropTable(
                name: "comments");

            migrationBuilder.DropTable(
                name: "crew");

            migrationBuilder.DropTable(
                name: "ExecutedScripts");

            migrationBuilder.DropTable(
                name: "follows");

            migrationBuilder.DropTable(
                name: "list_likes");

            migrationBuilder.DropTable(
                name: "list_movies");

            migrationBuilder.DropTable(
                name: "movie_genres");

            migrationBuilder.DropTable(
                name: "movie_images");

            migrationBuilder.DropTable(
                name: "movie_keywords");

            migrationBuilder.DropTable(
                name: "movie_likes");

            migrationBuilder.DropTable(
                name: "movie_origin_countries");

            migrationBuilder.DropTable(
                name: "movie_production_companies");

            migrationBuilder.DropTable(
                name: "movie_production_countries");

            migrationBuilder.DropTable(
                name: "movie_spoken_languages");

            migrationBuilder.DropTable(
                name: "movie_videos");

            migrationBuilder.DropTable(
                name: "review_likes");

            migrationBuilder.DropTable(
                name: "want_to_watch_films");

            migrationBuilder.DropTable(
                name: "watched_movies");

            migrationBuilder.DropTable(
                name: "people");

            migrationBuilder.DropTable(
                name: "list_entities");

            migrationBuilder.DropTable(
                name: "genres");

            migrationBuilder.DropTable(
                name: "keywords");

            migrationBuilder.DropTable(
                name: "production_companies");

            migrationBuilder.DropTable(
                name: "spoken_languages");

            migrationBuilder.DropTable(
                name: "reviews");

            migrationBuilder.DropTable(
                name: "countries");

            migrationBuilder.DropTable(
                name: "movies");

            migrationBuilder.DropTable(
                name: "users");
        }
    }
}
