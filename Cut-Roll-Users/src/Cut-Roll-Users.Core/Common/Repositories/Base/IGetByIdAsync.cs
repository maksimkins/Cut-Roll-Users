namespace Cut_Roll_Users.Core.Common.Repositories.Base;

public interface IGetByIdAsync<TId, TReturnEntity>
{
    Task<TReturnEntity> GetByIdAsync(TId id);
}
