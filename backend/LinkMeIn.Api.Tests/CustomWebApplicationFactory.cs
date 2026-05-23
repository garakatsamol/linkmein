using Microsoft.Extensions.Configuration;
using System;
using System.Linq;
using LinkMeIn.Api.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace LinkMeIn.Api.Tests
{
    public class CustomWebApplicationFactory<TStartup> : WebApplicationFactory<TStartup> where TStartup : class
    {
        public async Task ResetDatabaseAsync()
        {
            using var scope = Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<LinkMeInDbContext>();
            // Remove all data from tables
            db.PostMedia.RemoveRange(db.PostMedia);
            db.PublishAttempts.RemoveRange(db.PublishAttempts);
            db.Posts.RemoveRange(db.Posts);
            db.LinkedInConnections.RemoveRange(db.LinkedInConnections);
            db.OAuthStates.RemoveRange(db.OAuthStates);
            await db.SaveChangesAsync();
        }
        private readonly string _dbName = $"LinkMeInTests_{Guid.NewGuid()}";

        protected override IHost CreateHost(IHostBuilder builder)
        {
            builder.UseEnvironment("Testing");
            builder.ConfigureAppConfiguration((context, configBuilder) =>
            {
                var dict = new System.Collections.Generic.Dictionary<string, string>
                {
                    ["Database:Provider"] = "InMemory",
                    ["Database:InMemoryName"] = _dbName
                };
                configBuilder.AddInMemoryCollection(dict);
            });
            return base.CreateHost(builder);
        }

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            // No service removal or direct DbContext registration. Program.cs handles all registration.
        }
    }
}