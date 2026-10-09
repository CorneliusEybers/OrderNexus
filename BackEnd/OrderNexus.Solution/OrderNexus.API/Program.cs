
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using OrderNexus.Infrastructure;
using OrderNexus.Infrastructure.Persistence;
using OrderNexus.Infrastructure.Persistence.Seeding;
using OrderNexus.Application.Services;
using OrderNexus.API.Middleware;

var builder = WebApplication.CreateBuilder(args);

// ---------------------------------------------------------
// Database configuration
// ---------------------------------------------------------

string configuredConnectionString = builder.Configuration.GetConnectionString("DefaultConnection")
                                    ?? 
                                    throw new InvalidOperationException("DefaultConnection is missing from configuration.");

var sqliteConnectionBuilder = new SqliteConnectionStringBuilder(configuredConnectionString);

// Resolve the database path relative to the API project,
// regardless of the terminal's working directory.
string databasePath = Path.GetFullPath(sqliteConnectionBuilder.DataSource,
                                       builder.Environment.ContentRootPath);

string databaseDirectory = Path.GetDirectoryName(databasePath)
                           ?? 
                           throw new InvalidOperationException("Unable to determine the SQLite database directory.");

Directory.CreateDirectory(databaseDirectory);

sqliteConnectionBuilder.DataSource = databasePath;

// ---------------------------------------------------------
// Service registration
// ---------------------------------------------------------

builder.Services.AddControllers();

builder.Services.AddOpenApi();

builder.Services.AddInfrastructure(sqliteConnectionBuilder.ToString());

builder.Services.AddScoped<OrderService>();

builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<ApiExceptionHandler>();

// ---------------------------------------------------------
// Application
// ---------------------------------------------------------

var app = builder.Build();

// ---------------------------------------------------------
// Centralized exception handling
// ---------------------------------------------------------
app.UseExceptionHandler();

// ---------------------------------------------------------
// Database initialization
// ---------------------------------------------------------

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<OrderNexusDbContext>();

    // Apply any pending database migrations.
    await dbContext.Database.MigrateAsync();

    // Populate lookup tables after schema initialization.
    await DatabaseSeeder.SeedAsync(dbContext);
}

// ---------------------------------------------------------
// HTTP pipeline
// ---------------------------------------------------------

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();

    app.UseSwaggerUI(options => {
                                    options.SwaggerEndpoint("/openapi/v1.json", "OrderNexus API v1");

                                    options.RoutePrefix = "swagger";
                                });
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
