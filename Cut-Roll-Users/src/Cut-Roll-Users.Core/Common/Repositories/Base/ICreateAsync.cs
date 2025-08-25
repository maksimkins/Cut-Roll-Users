namespace Cut_Roll_Users.Core.Common.Repositories.Base;

public interface ICreateAsync<TEntity, TReturn> 
{
    Task<TReturn> CreateAsync(TEntity entity);
}
