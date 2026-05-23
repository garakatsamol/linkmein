using LinkMeIn.Api.Data;
using Microsoft.EntityFrameworkCore;

const string AngularDevelopmentCorsPolicy = "AngularDevelopment";

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
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
builder.Services.AddDbContext<LinkMeInDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.UseCors(AngularDevelopmentCorsPolicy);

app.MapGet("/api/health", () => Results.Ok(new
{
    status = "ok",
    service = "LinkMeIn.Api",
    timestamp = DateTimeOffset.UtcNow
}))
.WithName("GetHealth");

app.Run();

public partial class Program { }
