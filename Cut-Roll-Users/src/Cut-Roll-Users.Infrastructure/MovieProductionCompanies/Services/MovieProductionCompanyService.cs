namespace Cut_Roll_Users.Infrastructure.MovieProductionCompanies.Services;

using Cut_Roll_Users.Core.Common.Dtos;
using Cut_Roll_Users.Core.MovieProductionCompanies.Dtos;
using Cut_Roll_Users.Core.MovieProductionCompanies.Repositories;
using Cut_Roll_Users.Core.MovieProductionCompanies.Service;
using Cut_Roll_Users.Core.Movies.Dtos;
using Cut_Roll_Users.Core.Movies.Models;
using Cut_Roll_Users.Core.ProductionCompanies.Models;

public class MovieProductionCompanyService : IMovieProductionCompanyService
{
    private readonly IMovieProductionCompanyRepository _movieProductionCompanyRepository;

    public MovieProductionCompanyService(IMovieProductionCompanyRepository movieProductionCompanyRepository)
    {
        _movieProductionCompanyRepository = movieProductionCompanyRepository ?? throw new Exception($"{nameof(_movieProductionCompanyRepository)}");
    }

    public async Task<bool> BulkCreateMovieProductionCompanyAsync(IEnumerable<MovieProductionCompanyDto>? toCreate)
    {
        if (toCreate == null || !toCreate.Any())
            throw new ArgumentNullException($"there is no instances to create");

        foreach (var c in toCreate)
        {
            if (c == null)
                throw new ArgumentNullException("one of the object is null");
            if (c.MovieId == Guid.Empty)
                throw new ArgumentNullException($"missing {c.MovieId}");
            if (c.CompanyId == Guid.Empty)
                throw new ArgumentNullException($"missing {c.CompanyId}");

            var exists = await _movieProductionCompanyRepository.ExistsAsync(c);


            if (exists)
                throw new ArgumentException($"movie with id: {c.MovieId} has already possess company with id: {c.CompanyId}");
            
        }

        return await _movieProductionCompanyRepository.BulkCreateAsync(toCreate);
    }

    public async Task<bool> BulkDeleteMovieProductionCompanyAsync(IEnumerable<MovieProductionCompanyDto>? toDelete)
    {
        if (toDelete == null || !toDelete.Any())
            throw new ArgumentNullException($"there is no instances to create");
        
        foreach (var c in toDelete)
        {
            if (c == null)
                throw new ArgumentNullException("one of the object is null");
            if (c.MovieId == Guid.Empty)
                throw new ArgumentNullException($"missing {c.MovieId}");
            if (c.CompanyId == Guid.Empty)
                throw new ArgumentNullException($"missing {c.CompanyId}");

            var exists = await _movieProductionCompanyRepository.ExistsAsync(c);

            if (!exists)
                throw new ArgumentException($"movie with id: {c.MovieId} has not possess company with id: {c.CompanyId}"); 
        }

        return await _movieProductionCompanyRepository.BulkDeleteAsync(toDelete);
    }

    public async Task<Guid> CreateMovieProductionCompanyAsync(MovieProductionCompanyDto? dto)
    {
        if (dto == null)
            throw new ArgumentNullException("nothing to create");
        if (dto.MovieId == Guid.Empty)
            throw new ArgumentNullException($"missing {nameof(dto.MovieId)}");
        if (dto.CompanyId == Guid.Empty)
            throw new ArgumentNullException($"missing {nameof(dto.CompanyId)}");

        if (await _movieProductionCompanyRepository.ExistsAsync(dto))
            throw new ArgumentException($"movie with id: {dto.MovieId} has already possess company with id: {dto.CompanyId}");
            
        return await _movieProductionCompanyRepository.CreateAsync(dto) ??
            throw new InvalidOperationException(message: "could not add company to movie");
    }

    public async Task<Guid> DeleteMovieProductionCompanyAsync(MovieProductionCompanyDto? dto)
    {
        if (dto == null)
            throw new ArgumentNullException("nothing to delete");
        if (dto.MovieId == Guid.Empty)
            throw new ArgumentNullException($"missing {nameof(dto.MovieId)}");
        if (dto.CompanyId == Guid.Empty)
            throw new ArgumentNullException($"missing {nameof(dto.CompanyId)}");
        
        if (!await _movieProductionCompanyRepository.ExistsAsync(dto))
            throw new ArgumentException($"movie with id: {dto.MovieId} has not possess company with id: {dto.CompanyId}");

        return await _movieProductionCompanyRepository.DeleteAsync(dto) ??
            throw new InvalidOperationException(message: "could not remove company from movie");
    }

    public async Task<bool> DeleteMovieProductionCompanyRangeByMovieIdAsync(Guid? movieId)
    {
        if (movieId == null || movieId == Guid.Empty)
            throw new ArgumentNullException($"missing {nameof(movieId)}");
            
        return await _movieProductionCompanyRepository.DeleteRangeById(movieId.Value);
    }

    public async Task<IEnumerable<ProductionCompany>> GetCompaniesByMovieIdAsync(Guid? movieId)
    {
        if (movieId == null || movieId == Guid.Empty)
            throw new ArgumentNullException($"missing {nameof(movieId)}");
            
        return await _movieProductionCompanyRepository.GetCompaniesByMovieIdAsync(movieId.Value);
    }

    public async Task<PagedResult<MovieSimplifiedDto>> GetMoviesByCompanyIdAsync(MovieSearchByCompanyDto? dto)
    {
        if (dto == null)
            throw new ArgumentNullException($"missing {nameof(dto)}");
        if (dto.CompanyId == Guid.Empty)
            throw new ArgumentNullException($"missing {nameof(dto.CompanyId)}");
        if (dto.Name == null)
            throw new ArgumentNullException($"missing {nameof(dto.Name)}");


        return await _movieProductionCompanyRepository.GetMoviesByCompanyIdAsync(dto);
    }
}

