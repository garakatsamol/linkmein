using LinkMeIn.Api.Data;
using LinkMeIn.Api.Options;
using LinkMeIn.Api.Services;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;



var builder = WebApplication.CreateBuilder(args);
// Register AI suggestion service abstraction
builder.Services.AddScoped<IPostSuggestionService, MockPostSuggestionService>();

const string AngularDevelopmentCorsPolicy = "AngularDevelopment";

builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.Configure<MediaStorageOptions>(builder.Configuration.GetSection("MediaStorage"));
builder.Services.Configure<LinkedInOptions>(builder.Configuration.GetSection("LinkedIn"));
builder.Services.AddScoped<IMediaStorageService, LocalMediaStorageService>();
builder.Services.AddHttpClient<ILinkedInOAuthClient, LinkedInOAuthClient>();
builder.Services.AddHttpClient<ILinkedInPublishingClient, LinkedInPublishingClient>();
builder.Services
    .AddDataProtection()
    .SetApplicationName("LinkMeIn.Api");
builder.Services.AddScoped<ITokenEncryptionService, DataProtectionTokenEncryptionService>();
builder.Services.AddScoped<IPostPublishingService, PostPublishingService>();

builder.Services.AddCors(options =>
{
    options.AddPolicy(
        AngularDevelopmentCorsPolicy,
        policy =>
        {
            policy
                .WithOrigins("http://localhost:4200", "http://127.0.0.1:4200")
                .AllowAnyHeader()
                .AllowAnyMethod();
        });
});

var dbProvider = builder.Configuration["Database:Provider"];
var isTesting = builder.Environment.IsEnvironment("Testing");

if (string.Equals(dbProvider, "InMemory", StringComparison.OrdinalIgnoreCase) || isTesting)
{
    var inMemoryDatabaseName = builder.Configuration["Database:InMemoryName"] ?? "LinkMeInTests";
    builder.Services.AddDbContext<LinkMeInDbContext>(options =>
        options.UseInMemoryDatabase(inMemoryDatabaseName));
}
else
{
    builder.Services.AddDbContext<LinkMeInDbContext>(options =>
        options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
}

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.UseCors(AngularDevelopmentCorsPolicy);

app.MapControllers();

app.MapGet("/api/health", () => Results.Ok(new
{
    status = "ok",
    service = "LinkMeIn.Api",
    timestamp = DateTimeOffset.UtcNow
}))
.WithName("GetHealth");

app.Run();

public partial class Program
{
}
