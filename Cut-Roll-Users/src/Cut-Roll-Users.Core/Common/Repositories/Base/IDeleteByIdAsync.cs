namespace Cut_Roll_Users.Core.Common.Repositories.Base;
public interface IDeleteByIdAsync<TId, TReturn> 
{
    Task<TReturn> DeleteByIdAsync(TId id);
}