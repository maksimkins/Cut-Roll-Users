namespace Cut_Roll_Users.Infrastructure.Users.BackgroundServices;

using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Hosting;
using Cut_Roll_Users.Core.Common.Options;
using Cut_Roll_Users.Core.Common.BackgroundServices;
using Cut_Roll_Users.Core.Users.Repositories;
using Cut_Roll_Users.Core.Users.Dtos;

public class UserRabbitMqService : BaseRabbitMqService, IHostedService
{
    public UserRabbitMqService(IOptions<RabbitMqOptions> optionsSnapshot, IServiceScopeFactory serviceScopeFactory) :
        base(optionsSnapshot, serviceScopeFactory)
    {
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        base.StartListening("user_create_users", async message => {
            using (var scope = base.serviceScopeFactory.CreateScope())
            {
                var userRepository = scope.ServiceProvider.GetRequiredService<IUserRepository>();

                var newUser = JsonSerializer.Deserialize<UserCreateDto>(message)!;

                await userRepository.CreateAsync(newUser);
            }
        });

        base.StartListening("user_update_users", async message => {

            using (var scope = base.serviceScopeFactory.CreateScope())
            {
                var userRepository = scope.ServiceProvider.GetRequiredService<IUserRepository>();

                var updateDto = JsonSerializer.Deserialize<UserUpdateDto>(message)!;

                if (updateDto.Id is null)
                {
                    throw new ArgumentException("User ID cannot be null for update operation.");
                }

                var userToUpdate = await userRepository.GetByIdAsync(updateDto.Id) ?? throw new ArgumentException($"there is no user with id: {updateDto.Id}");

                userToUpdate.Email = updateDto.Email is null ? userToUpdate.Email : updateDto.Email;
                userToUpdate.Username = updateDto.UserName is null ? userToUpdate.Username : updateDto.UserName;
                await userRepository.UpdateAsync(new UserUpdateDto
                {
                    Id = userToUpdate.Id,
                    RoleId = updateDto.RoleId,
                    UserName = userToUpdate.Username,
                    Email = userToUpdate.Email
                });
            }
        });

        base.StartListening("user_update_avatar_users", async message => {
            using (var scope = base.serviceScopeFactory.CreateScope())
            {
                var userRepository = scope.ServiceProvider.GetRequiredService<IUserRepository>();

                var dto = JsonSerializer.Deserialize<UserUpdateAvatarDto>(message)!;

                await userRepository.UpdateAvatarAsync(dto);
            }
        });


        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}
