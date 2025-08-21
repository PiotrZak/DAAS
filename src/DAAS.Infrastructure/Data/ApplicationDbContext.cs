using DAAS.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace DAAS.Infrastructure.Data;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : DbContext(options)
{

    public DbSet<User> Users { get; set; }
    public DbSet<Document> Documents { get; set; }
    public DbSet<AccessRequest> AccessRequests { get; set; }
    public DbSet<Decision> Decisions { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // User configuration
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Email).IsRequired().HasMaxLength(255);
            entity.HasIndex(e => e.Email).IsUnique();
            entity.Property(e => e.Role).HasConversion<int>();
        });

        // Document configuration
        modelBuilder.Entity<Document>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Title).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Description).HasMaxLength(1000);
            entity.Property(e => e.CreatedAt).IsRequired();
        });

        // AccessRequest configuration
        modelBuilder.Entity<AccessRequest>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Reason).IsRequired().HasMaxLength(500);
            entity.Property(e => e.AccessType).HasConversion<int>();
            entity.Property(e => e.Status).HasConversion<int>();
            entity.Property(e => e.RequestedAt).IsRequired();

            entity.HasOne(e => e.User)
                .WithMany(u => u.AccessRequests)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Document)
                .WithMany(d => d.AccessRequests)
                .HasForeignKey(e => e.DocumentId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // Decision configuration
        modelBuilder.Entity<Decision>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Comment).HasMaxLength(500);
            entity.Property(e => e.DecidedAt).IsRequired();

            entity.HasOne(e => e.AccessRequest)
                .WithOne(ar => ar.Decision)
                .HasForeignKey<Decision>(e => e.AccessRequestId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Approver)
                .WithMany(u => u.Decisions)
                .HasForeignKey(e => e.ApproverId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        base.OnModelCreating(modelBuilder);
    }
}