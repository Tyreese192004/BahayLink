using Microsoft.EntityFrameworkCore;
using Inquiry.Data;
using Inquiry.Repositories;
using Inquiry.Services;

// 0. Auto-load .env file if it exists
void LoadEnv(string filePath)
{
    if (!File.Exists(filePath)) return;
    foreach (var line in File.ReadAllLines(filePath))
    {
        var trimmed = line.Trim();
        if (string.IsNullOrWhiteSpace(trimmed) || trimmed.StartsWith("#")) continue;
        var parts = trimmed.Split('=', 2);
        if (parts.Length == 2)
        {
            var key = parts[0].Trim();
            var value = parts[1].Trim().Trim('"').Trim('\'');
            Environment.SetEnvironmentVariable(key, value);
        }
    }
}

var currentDir = Directory.GetCurrentDirectory();
var envFile = Path.Combine(currentDir, ".env");
if (!File.Exists(envFile))
{
    var parentEnv = Path.Combine(Directory.GetParent(currentDir)?.FullName ?? "", ".env");
    if (File.Exists(parentEnv)) envFile = parentEnv;
}
LoadEnv(envFile);

var builder = WebApplication.CreateBuilder(args);
builder.Configuration.AddEnvironmentVariables();

// 1. Database Configuration (PostgreSQL)
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
if (string.IsNullOrWhiteSpace(connectionString) || connectionString.Contains("YOUR_POSTGRES_PASSWORD_HERE"))
{
    var host = builder.Configuration["DB_HOST"] ?? "localhost";
    var port = builder.Configuration["DB_PORT"] ?? "5432";
    var db = builder.Configuration["DB_NAME"] ?? "InquiryDb";
    var user = builder.Configuration["DB_USER"] ?? "postgres";
    var pass = builder.Configuration["DB_PASSWORD"] ?? "";
    connectionString = $"Host={host};Port={port};Database={db};Username={user};Password={pass}";
}

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString));

// 2. Dependency Injection (DI) Registration
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IPropertyRepository, PropertyRepository>();
builder.Services.AddScoped<IPropertyService, PropertyService>();
builder.Services.AddScoped<IInquiryRepository, InquiryRepository>();
builder.Services.AddScoped<IInquiryService, InquiryService>();

// 3. CORS Configuration (Allows frontend to communicate with API)
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// 4. Controllers & Swagger
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Serve Frontend Static Files (HTML, CSS, JS) mula sa wwwroot
app.UseDefaultFiles();
app.UseStaticFiles();

app.UseCors("AllowAll");

app.UseAuthorization();

app.MapControllers();

// 5. Seed Initial Database Data & Admin Account mula sa .env
await DataSeeder.SeedAsync(app.Services, app.Configuration);

app.Run();
