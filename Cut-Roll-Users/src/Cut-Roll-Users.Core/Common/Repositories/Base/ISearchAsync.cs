namespace Cut_Roll_Users.Core.Common.Repositories.Base;

public interface ISearchAsync<TRequest, TResponse>
{
    Task<TResponse> SearchAsync(TRequest request);
}

