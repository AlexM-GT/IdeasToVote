using IdeasToVote.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace IdeasToVote.Api.Data;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : DbContext(options)
{
    public DbSet<User> Users => Set<User>();
    public DbSet<Idea> Ideas => Set<Idea>();
    public DbSet<Vote> Votes => Set<Vote>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("Users");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Username).HasMaxLength(50).IsRequired();
            entity.Property(x => x.Email).HasMaxLength(255).IsRequired();
            entity.Property(x => x.PasswordHash).HasMaxLength(512).IsRequired();
            entity.Property(x => x.Salt).HasMaxLength(128).IsRequired();
            entity.Property(x => x.Icon).HasMaxLength(255);
            entity.Property(x => x.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
            entity.HasIndex(x => x.Username).IsUnique();
            entity.HasIndex(x => x.Email).IsUnique();
        });

        modelBuilder.Entity<Idea>(entity =>
        {
            entity.ToTable("Ideas");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Title).HasMaxLength(200).IsRequired();
            entity.Property(x => x.Description).HasMaxLength(4000);
            entity.Property(x => x.CreatedAt).HasDefaultValueSql("GETUTCDATE()");

            entity.HasOne(x => x.User)
                .WithMany(u => u.Ideas)
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Vote>(entity =>
        {
            entity.ToTable("Votes", tableBuilder =>
                tableBuilder.HasCheckConstraint("CK_Votes_Value_Range", "[Value] >= 1 AND [Value] <= 5"));
            entity.HasKey(x => x.Id);
            entity.Property(x => x.CreatedAt).HasDefaultValueSql("GETUTCDATE()");

            entity.HasOne(x => x.Idea)
                .WithMany(i => i.Votes)
                .HasForeignKey(x => x.IdeaId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(x => x.User)
                .WithMany(u => u.Votes)
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(x => new { x.IdeaId, x.UserId }).IsUnique();
        });
    }
}
