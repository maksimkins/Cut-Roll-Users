using Cut_Roll_Users.Core.ListMovies.Dtos;
using Cut_Roll_Users.Core.ListMovies.Repositories;
using Cut_Roll_Users.Core.ListMovies.Services;

namespace Cut_Roll_Users.Infrastructure.ListMovies.Services;

public class ListMovieService : IListMovieService
{
    private readonly IListMovieRepository _listMovieRepository;

    public ListMovieService(IListMovieRepository listMovieRepository)
    {
        _listMovieRepository = listMovieRepository;
    }

    public async Task<Guid> AddMovieToListAsync(ListMovieDto? listMovieDto)
    {
        if (listMovieDto == null)
            throw new ArgumentNullException(nameof(listMovieDto), "List movie data cannot be null.");
        
        if (listMovieDto.ListId == Guid.Empty)
            throw new ArgumentException("List ID cannot be empty.", nameof(listMovieDto.ListId));
        
        if (listMovieDto.MovieId == Guid.Empty)
            throw new ArgumentException("Movie ID cannot be empty.", nameof(listMovieDto.MovieId));

     
        var isAlreadyInList = await _listMovieRepository.IsMovieInListAsync(listMovieDto.ListId, listMovieDto.MovieId);
        if (isAlreadyInList)
            return listMovieDto.ListId; 

        var result = await _listMovieRepository.CreateAsync(listMovieDto);
        return result ?? throw new InvalidOperationException("Failed to add movie to list.");
    }

    public async Task<Guid> RemoveMovieFromListAsync(ListMovieDto? listMovieDto)
    {
        if (listMovieDto == null)
            throw new ArgumentNullException(nameof(listMovieDto), "List movie data cannot be null.");
        
        if (listMovieDto.ListId == Guid.Empty)
            throw new ArgumentException("List ID cannot be empty.", nameof(listMovieDto.ListId));
        
        if (listMovieDto.MovieId == Guid.Empty)
            throw new ArgumentException("Movie ID cannot be empty.", nameof(listMovieDto.MovieId));

        
        var isInList = await _listMovieRepository.IsMovieInListAsync(listMovieDto.ListId, listMovieDto.MovieId);
        if (!isInList)
            return listMovieDto.ListId; 

        var result = await _listMovieRepository.DeleteAsync(listMovieDto);
        if (result == null)
            throw new InvalidOperationException($"ListMovie not found for ListId: {listMovieDto.ListId} and MovieId: {listMovieDto.MovieId}");
        
        return result.Value;
    }

    public async Task<bool> BulkCreateAsync(IEnumerable<ListMovieDto>? toCreate)
    {
        if (toCreate == null)
            throw new ArgumentNullException(nameof(toCreate), "Bulk create data cannot be null.");

        if (!toCreate.Any())
            return true; 

        
        foreach (var item in toCreate)
        {
            if (item.ListId == Guid.Empty)
                throw new ArgumentException("List ID cannot be empty in bulk create.");
            
            if (item.MovieId == Guid.Empty)
                throw new ArgumentException("Movie ID cannot be empty in bulk create.");
        }

        return await _listMovieRepository.BulkCreateAsync(toCreate);
    }

    public async Task<bool> BulkDeleteAsync(IEnumerable<ListMovieDto>? toDelete)
    {
        if (toDelete == null)
            throw new ArgumentNullException(nameof(toDelete), "Bulk delete data cannot be null.");

        if (!toDelete.Any())
            return true;

        
        foreach (var item in toDelete)
        {
            if (item.ListId == Guid.Empty)
                throw new ArgumentException("List ID cannot be empty in bulk delete.");
            
            if (item.MovieId == Guid.Empty)
                throw new ArgumentException("Movie ID cannot be empty in bulk delete.");
        }

        return await _listMovieRepository.BulkDeleteAsync(toDelete);
    }

    public async Task<bool> IsMovieInListAsync(Guid? listId, Guid? movieId)
    {
        if (!listId.HasValue || listId.Value == Guid.Empty)
            throw new ArgumentException("List ID cannot be null or empty.", nameof(listId));
        
        if (!movieId.HasValue || movieId.Value == Guid.Empty)
            throw new ArgumentException("Movie ID cannot be null or empty.", nameof(movieId));

        return await _listMovieRepository.IsMovieInListAsync(listId.Value, movieId.Value);
    }

    public async Task<int> GetMovieCountByListIdAsync(Guid? listId)
    {
        if (!listId.HasValue || listId.Value == Guid.Empty)
            throw new ArgumentException("List ID cannot be null or empty.", nameof(listId));

        return await _listMovieRepository.GetMovieCountByListIdAsync(listId.Value);
    }
}
