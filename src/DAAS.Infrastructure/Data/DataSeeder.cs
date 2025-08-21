using DAAS.Domain.Entities;
using DAAS.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace DAAS.Infrastructure.Data;

public static class DataSeeder
{
    public static async Task SeedAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<ApplicationDbContext>>();

        try
        {
            await context.Database.EnsureCreatedAsync();

            if (!await context.Users.AnyAsync())
            {
                var users = new List<User>
                {
                    new() { Name = "John Doe", Email = "john.doe@company.com", Role = UserRole.User },
                    new() { Name = "Jane Smith", Email = "jane.smith@company.com", Role = UserRole.User },
                    new() { Name = "Bob Johnson", Email = "bob.johnson@company.com", Role = UserRole.Approver },
                    new() { Name = "Alice Brown", Email = "alice.brown@company.com", Role = UserRole.Admin }
                };

                context.Users.AddRange(users);
                await context.SaveChangesAsync();
                logger.LogInformation("Seeded {Count} users", users.Count);
            }

            if (!await context.Documents.AnyAsync())
            {
                var documents = new List<Document>
                {
                    new() 
                    { 
                        Title = "Company Financial Report 2024", 
                        Description = "Annual financial statements and performance metrics", 
                        CreatedAt = DateTime.UtcNow.AddDays(-30) 
                    },
                    new() 
                    { 
                        Title = "Employee Handbook", 
                        Description = "HR policies and procedures for all employees", 
                        CreatedAt = DateTime.UtcNow.AddDays(-60) 
                    },
                    new() 
                    { 
                        Title = "Product Roadmap 2025", 
                        Description = "Strategic product development plan for next year", 
                        CreatedAt = DateTime.UtcNow.AddDays(-15) 
                    },
                    new() 
                    { 
                        Title = "Security Compliance Guidelines", 
                        Description = "Information security policies and compliance requirements", 
                        CreatedAt = DateTime.UtcNow.AddDays(-45) 
                    }
                };

                context.Documents.AddRange(documents);
                await context.SaveChangesAsync();
                logger.LogInformation("Seeded {Count} documents", documents.Count);
            }

            // Seed some sample access requests for demonstration
            if (!await context.AccessRequests.AnyAsync())
            {
                var users = await context.Users.ToListAsync();
                var documents = await context.Documents.ToListAsync();

                if (users.Count >= 2 && documents.Count >= 2)
                {
                    var requests = new List<AccessRequest>
                    {
                        new()
                        {
                            UserId = users[0].Id,
                            DocumentId = documents[0].Id,
                            Reason = "Need to review financial data for quarterly presentation",
                            AccessType = AccessType.Read,
                            Status = RequestStatus.Pending,
                            RequestedAt = DateTime.UtcNow.AddHours(-2)
                        },
                        new()
                        {
                            UserId = users[1].Id,
                            DocumentId = documents[1].Id,
                            Reason = "Updating employee onboarding process",
                            AccessType = AccessType.Edit,
                            Status = RequestStatus.Pending,
                            RequestedAt = DateTime.UtcNow.AddHours(-1)
                        }
                    };

                    context.AccessRequests.AddRange(requests);
                    await context.SaveChangesAsync();
                    logger.LogInformation("Seeded {Count} access requests", requests.Count);
                }
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while seeding the database");
        }
    }
}