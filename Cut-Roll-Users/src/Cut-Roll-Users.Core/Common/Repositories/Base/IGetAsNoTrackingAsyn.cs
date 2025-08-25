

namespace Cut_Roll_Users.Core.Common.Repositories.Base;
public interface IGetAsNoTrackingAsync<TEntity, TId>
{
    Task<TEntity?> GetAsNoTrackingAsync(TId id);
}