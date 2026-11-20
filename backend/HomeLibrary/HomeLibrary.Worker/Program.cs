using HomeLibrary.Persistence.Data;
using HomeLibrary.Worker;
using HomeLibrary.Worker.Configuration;
using HomeLibrary.Worker.Services;
using Microsoft.EntityFrameworkCore;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddDbContextFactory<LibraryDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("Library")));

builder.Services.AddSingleton<BookPersistenceService>();
builder.Services.AddSingleton<BookMessageProcessor>();
builder.Services.AddHostedService<Worker>();


builder.Services.AddOptions<RabbitMqOptions>()
    .Bind(builder.Configuration.GetSection(RabbitMqOptions.SectionName))
    .Validate(options =>
        !string.IsNullOrWhiteSpace(options.HostName) &&
        !string.IsNullOrWhiteSpace(options.UserName) &&
        !string.IsNullOrWhiteSpace(options.Password),
        "RabbitMQ configuration is incomplete.")
    .ValidateOnStart();

var host = builder.Build();

var logger = host.Services.GetRequiredService<ILogger<Program>>();

while (true)
{
    try
    {
        await using var scope = host.Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<LibraryDbContext>();
        await dbContext.Database.MigrateAsync();

        logger.LogInformation("Database migrations applied successfully.");
        break;
    }
    catch (Exception ex)
    {
        logger.LogWarning(ex, "PostgreSQL is not ready. Retrying in 5 seconds.");
        await Task.Delay(TimeSpan.FromSeconds(5));
    }
}

await host.RunAsync();
