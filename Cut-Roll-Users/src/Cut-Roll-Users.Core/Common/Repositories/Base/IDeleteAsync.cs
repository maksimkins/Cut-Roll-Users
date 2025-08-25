namespace Cut_Roll_Users.Core.Common.Repositories.Base;

public interface IDeleteAsync<TEntity, TReturn>
{
    Task<TReturn> DeleteAsync(TEntity entity);
}
