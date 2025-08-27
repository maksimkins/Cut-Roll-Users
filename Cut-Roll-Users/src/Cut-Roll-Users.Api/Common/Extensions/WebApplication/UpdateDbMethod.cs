namespace Cut_Roll_Users.Api.Common.Extensions.WebApplication;

using Cut_Roll_Users.Infrastructure.Common.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Builder;
using Cut_Roll_Users.Core.Common.Models;

public static class UpdateDbContextMethod
{
    public async static Task UpdateDbContext(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var services = scope.ServiceProvider;
        var dbContext = services.GetRequiredService<UsersDbContext>();
        var logger = services.GetRequiredService<ILoggerFactory>().CreateLogger("DBUpdate");

        await dbContext.Database.MigrateAsync();

        dbContext.Database.ExecuteSqlRaw(
            @"CREATE TABLE IF NOT EXISTS ""ExecutedScripts"" (
                ""ScriptName"" TEXT PRIMARY KEY,
                ""ExecutedAt"" TIMESTAMP DEFAULT now()
            );"
        );


        var sqlFolder = Path.Combine(AppContext.BaseDirectory, "SqlScripts");

        Console.Write(sqlFolder);

        if (!Directory.Exists(sqlFolder))
        {
            logger.LogWarning("SqlScripts directory not found: " + sqlFolder);
            return;
        }

        var sqlFiles = Directory.GetFiles(sqlFolder, "*.sql").OrderBy(f => f);

        foreach (var file in sqlFiles)
        {
            var scriptName = Path.GetFileName(file);

            var wasExecuted = dbContext.ExecutedScripts.Any(s => s.ScriptName == scriptName);

            if (wasExecuted)
            {
                logger.LogInformation($"Skipped already executed script: {scriptName}");
                continue;
            }

            var scriptText = File.ReadAllText(file);
            try
            {
                dbContext.Database.ExecuteSqlRaw(scriptText);
                dbContext.ExecutedScripts.Add(new ExecutedScript
                {
                    ScriptName = scriptName,
                    ExecutedAt = DateTime.UtcNow
                });
                dbContext.SaveChanges();
                logger.LogInformation($"Successfully executed script: {scriptName}");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"\n\n\n\n\n\n\nFailed to execute script: {scriptName}\n\n\n\n\n\n");
                throw;
            }
        }
    }
}