using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.DependencyInjection;
using UrlShortner.Domain.Data;

namespace UrlShortner.Domain;

public static class MigrationService
{
    public static void ApplyMigration(this IServiceProvider services)
    {
        // Automatically create and apply migrations
        using var scope = services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        try
        {
            if (dbContext.Database.CanConnect() == false)
            {
                Console.WriteLine("Database does not exist. Creating...");
                dbContext.Database.EnsureCreated();
            }

            // Automatically create the initial migration if none exists
            var migrationsAssembly = dbContext.GetType().Assembly;
            var migrator = dbContext.GetService<IMigrator>();

            if (dbContext.Database.GetPendingMigrations().Any() == false)
            {
                Console.WriteLine("No migrations found. Creating the initial migration...");
                var initialMigrationName = "InitialCreate";
                var command = $"dotnet ef migrations add {initialMigrationName}";
                System.Diagnostics.Process.Start("cmd.exe", $"/C {command}").WaitForExit();
            }

            // Apply pending migrations
            dbContext.Database.Migrate();
            Console.WriteLine("Database migrations applied successfully.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An error occurred during migration: {ex.Message}");
        }
    }
}
