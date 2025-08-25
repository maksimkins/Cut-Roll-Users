namespace Cut_Roll_Users.Core.Common.Repositories.Base;

public interface IDeleteRangeById<TId, TReturn>
{
    Task<TReturn> DeleteRangeById(TId id);
}
