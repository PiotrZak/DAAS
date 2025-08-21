using DAAS.Application.Handlers;
using DAAS.Application.Notifications;
using DAAS.Domain.Interfaces;
using DAAS.Infrastructure.Data;
using DAAS.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "DAAS - Document Access Approval System API",
        Version = "v1",
        Description = "CQRS-based REST API for managing document access requests with approval workflow and notifications"
    });
    
    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        c.IncludeXmlComments(xmlPath);
    }
});

// Database configuration
builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    if (builder.Environment.IsDevelopment())
    {
        // Use SQLite for development
        options.UseSqlite("Data Source=document_approval.db");
    }
    else
    {
        // Use InMemory for demonstration/testing
        options.UseInMemoryDatabase("DocumentApprovalDb");
    }
});

// Register repositories
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IDocumentRepository, DocumentRepository>();
builder.Services.AddScoped<IAccessRequestRepository, AccessRequestRepository>();
builder.Services.AddScoped<IDecisionRepository, DecisionRepository>();

// Register MediatR for CQRS
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(GetAllUsersQueryHandler).Assembly));

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "DAAS API V1");
        c.RoutePrefix = string.Empty; // Set Swagger UI at the app's root
    });
}

app.UseHttpsRedirection();
app.MapControllers();

// Seed the database
await DataSeeder.SeedAsync(app.Services);

app.Run();
