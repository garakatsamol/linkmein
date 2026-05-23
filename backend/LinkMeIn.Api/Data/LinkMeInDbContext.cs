using LinkMeIn.Api.Entities;
using Microsoft.EntityFrameworkCore;

namespace LinkMeIn.Api.Data;

public class LinkMeInDbContext(DbContextOptions<LinkMeInDbContext> options) : DbContext(options)
{
    public DbSet<Post> Posts => Set<Post>();
    public DbSet<PostMedia> PostMedia => Set<PostMedia>();
    public DbSet<LinkedInConnection> LinkedInConnections => Set<LinkedInConnection>();
    public DbSet<PublishAttempt> PublishAttempts => Set<PublishAttempt>();
    public DbSet<OAuthState> OAuthStates => Set<OAuthState>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Post>(entity =>
        {
            entity.ToTable("Posts");
            entity.Property(post => post.OwnerId).HasMaxLength(128).IsRequired();
            entity.Property(post => post.Title).HasMaxLength(200).IsRequired();
            entity.Property(post => post.Content).IsRequired();
            entity.Property(post => post.Status).HasConversion<string>().HasMaxLength(32).IsRequired();
            entity.Property(post => post.LinkedInPostId).HasMaxLength(256);
            entity.Property(post => post.RowVersion).IsRowVersion();
            entity.HasMany(post => post.Media).WithOne(media => media.Post).HasForeignKey(media => media.PostId);
            entity.HasMany(post => post.PublishAttempts).WithOne(attempt => attempt.Post).HasForeignKey(attempt => attempt.PostId);
            entity.HasIndex(post => new { post.OwnerId, post.Status });
            entity.HasIndex(post => post.ScheduledFor);
        });

        modelBuilder.Entity<PostMedia>(entity =>
        {
            entity.ToTable("PostMedia");
            entity.Property(media => media.OwnerId).HasMaxLength(128).IsRequired();
            entity.Property(media => media.FileName).HasMaxLength(255).IsRequired();
            entity.Property(media => media.ContentType).HasMaxLength(100).IsRequired();
            entity.Property(media => media.StoragePath).HasMaxLength(500).IsRequired();
            entity.Property(media => media.LinkedInAssetUrn).HasMaxLength(500);
            entity.HasIndex(media => new { media.OwnerId, media.PostId });
        });

        modelBuilder.Entity<LinkedInConnection>(entity =>
        {
            entity.ToTable("LinkedInConnections");
            entity.Property(connection => connection.OwnerId).HasMaxLength(128).IsRequired();
            entity.Property(connection => connection.LinkedInMemberId).HasMaxLength(128);
            entity.Property(connection => connection.DisplayName).HasMaxLength(200);
            entity.Property(connection => connection.AccessTokenEncrypted).IsRequired();
            entity.Property(connection => connection.Scopes).HasMaxLength(500).IsRequired();
            entity.HasIndex(connection => connection.OwnerId);
            entity.HasIndex(connection => connection.LinkedInMemberId);
        });

        modelBuilder.Entity<PublishAttempt>(entity =>
        {
            entity.ToTable("PublishAttempts");
            entity.Property(attempt => attempt.OwnerId).HasMaxLength(128).IsRequired();
            entity.Property(attempt => attempt.Status).HasConversion<string>().HasMaxLength(32).IsRequired();
            entity.Property(attempt => attempt.LinkedInPostId).HasMaxLength(256);
            entity.HasIndex(attempt => new { attempt.OwnerId, attempt.PostId });
            entity.HasIndex(attempt => attempt.AttemptedAt);
        });

        modelBuilder.Entity<OAuthState>(entity =>
        {
            entity.ToTable("OAuthStates");
            entity.Property(state => state.OwnerId).HasMaxLength(128).IsRequired();
            entity.Property(state => state.State).HasMaxLength(256).IsRequired();
            entity.Property(state => state.CodeVerifierHash).HasMaxLength(256);
            entity.Property(state => state.ReturnUrl).HasMaxLength(500);
            entity.HasIndex(state => state.State).IsUnique();
            entity.HasIndex(state => new { state.OwnerId, state.ExpiresAt });
        });
    }
}
