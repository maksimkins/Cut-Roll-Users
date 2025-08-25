

namespace Cut_Roll_Users.Core.Common.Services;
public interface IMessageBrokerService
{
    public Task PushAsync<T>(string destination, T obj);
}