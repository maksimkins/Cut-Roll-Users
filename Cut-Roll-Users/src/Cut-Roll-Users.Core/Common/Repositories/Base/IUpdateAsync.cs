namespace Cut_Roll_Users.Core.Common.Repositories.Base;

public interface IUpdateAsync<TEntity, TReturn> 
{
    Task<TReturn> UpdateAsync(TEntity entity);
}
