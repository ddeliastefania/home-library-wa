using HomeLibrary.API.Configuration;
using HomeLibrary.API.Interfaces;
using HomeLibrary.API.Parsing;
using HomeLibrary.API.Services;
using HomeLibrary.Persistence.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCors(options =>
{
    options.AddPolicy("AngularDev", policy =>
    {
        policy.WithOrigins("http://localhost:4200")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

builder.Services.AddControllers();
builder.Services.AddOpenApi();

builder.Services.AddDbContext<LibraryDbContext>(options =>
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("Library")));

builder.Services.AddScoped<IBookService, BookService>();
builder.Services.AddScoped<CsvBookParser>();
builder.Services.AddScoped<BookImportService>();
builder.Services.AddScoped<IBookMessagePublisher, RabbitMqPublisher>();

builder.Services
    .AddOptions<RabbitMqOptions>()
    .Bind(builder.Configuration.GetSection(RabbitMqOptions.SectionName))
    .Validate(
        options =>
            !string.IsNullOrWhiteSpace(options.HostName) &&
            !string.IsNullOrWhiteSpace(options.UserName) &&
            !string.IsNullOrWhiteSpace(options.Password),
        "RabbitMQ configuration is incomplete.")
    .ValidateOnStart();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseCors("AngularDev");
app.UseAuthorization();

app.MapControllers();

app.Run();
