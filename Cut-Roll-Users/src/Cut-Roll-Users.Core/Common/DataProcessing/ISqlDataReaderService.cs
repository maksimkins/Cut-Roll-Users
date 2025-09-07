

using Cut_Roll_Users.Core.Common.DataProcessing.Models;

namespace Cut_Roll_Users.Core.Common.DataProcessing;
public interface ISqlDataReaderService
{
    Task<List<SqlMovieData>> ExtractMovieDataBatchAsync(int offset, int limit);
    Task<SqlMovieData?> ExtractMovieDataByIdAsync(Guid movieId);
    Task<int> GetTotalMovieCountAsync();
}