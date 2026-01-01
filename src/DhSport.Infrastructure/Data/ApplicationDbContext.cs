using System.Reflection;
using DhSport.Domain.Entities.Business;
using DhSport.Domain.Entities.Common;
using DhSport.Domain.Entities.Content;
using DhSport.Domain.Entities.Features;
using DhSport.Domain.Entities.Site;
using DhSport.Domain.Entities.UserManagement;
using Microsoft.EntityFrameworkCore;

namespace DhSport.Infrastructure.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    // User Management
    public DbSet<User> Users => Set<User>();
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<UserRoleMap> UserRoleMaps => Set<UserRoleMap>();

    // Content
    public DbSet<BoardType> BoardTypes => Set<BoardType>();
    public DbSet<Board> Boards => Set<Board>();
    public DbSet<PostType> PostTypes => Set<PostType>();
    public DbSet<Post> Posts => Set<Post>();
    public DbSet<PostFile> PostFiles => Set<PostFile>();
    public DbSet<Comment> Comments => Set<Comment>();
    public DbSet<PostLike> PostLikes => Set<PostLike>();
    public DbSet<CommentLike> CommentLikes => Set<CommentLike>();
    public DbSet<PostRevision> PostRevisions => Set<PostRevision>();
    public DbSet<RelatedPost> RelatedPosts => Set<RelatedPost>();

    // Features
    public DbSet<FeatureType> FeatureTypes => Set<FeatureType>();
    public DbSet<AddFeature> AddFeatures => Set<AddFeature>();
    public DbSet<LikeLog> LikeLogs => Set<LikeLog>();
    public DbSet<ReadLog> ReadLogs => Set<ReadLog>();

    // Site
    public DbSet<Menu> Menus => Set<Menu>();
    public DbSet<MediaLib> MediaLibs => Set<MediaLib>();
    public DbSet<SiteConfig> SiteConfigs => Set<SiteConfig>();
    public DbSet<Notification> Notifications => Set<Notification>();
    public DbSet<SiteLog> SiteLogs => Set<SiteLog>();

    // Business
    public DbSet<Reservation> Reservations => Set<Reservation>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Apply all entity configurations from current assembly
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

        // Global query filter for soft delete
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (typeof(ISoftDeletable).IsAssignableFrom(entityType.ClrType))
            {
                var method = typeof(ApplicationDbContext)
                    .GetMethod(nameof(SetSoftDeleteFilter), BindingFlags.NonPublic | BindingFlags.Static)!
                    .MakeGenericMethod(entityType.ClrType);
                method.Invoke(null, new object[] { modelBuilder });
            }
        }
    }

    private static void SetSoftDeleteFilter<T>(ModelBuilder modelBuilder) where T : class, ISoftDeletable
    {
        modelBuilder.Entity<T>().HasQueryFilter(e => !e.IsDeleted);
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var entries = ChangeTracker.Entries<IAuditableEntity>();

        foreach (var entry in entries)
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    entry.Entity.CreateDttm = DateTime.UtcNow;
                    break;
                case EntityState.Modified:
                    entry.Entity.UpdateDttm = DateTime.UtcNow;
                    break;
            }
        }

        return await base.SaveChangesAsync(cancellationToken);
    }
}
