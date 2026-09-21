using Microsoft.EntityFrameworkCore;
using TaskBridge.Application.Abstractions;
using TaskBridge.Domain.Entities;

namespace TaskBridge.Infrastructure.Persistence;

/// <summary>EF Core database context for TaskBridge.</summary>
public sealed class TaskBridgeDbContext : DbContext, IUnitOfWork
{
    /// <summary>Initializes the database context.</summary>
    public TaskBridgeDbContext(DbContextOptions<TaskBridgeDbContext> options)
        : base(options)
    {
    }

    /// <summary>Gets the projects table.</summary>
    public DbSet<Project> Projects => Set<Project>();

    /// <summary>Gets the immutable audit entries table.</summary>
    public DbSet<AuditEntry> AuditEntries => Set<AuditEntry>();

    /// <summary>Gets the notifications table.</summary>
    public DbSet<Notification> Notifications => Set<Notification>();

    /// <inheritdoc />
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        var project = modelBuilder.Entity<Project>();

        project.HasKey(item => item.Id);
        project.Property(item => item.Name).HasMaxLength(200).IsRequired();
        project.Property(item => item.Description).HasMaxLength(2000);
        project.Property(item => item.Status).HasConversion<int>().IsRequired();
        project.HasIndex(item => new { item.OrganisationId, item.TeamId });

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(TaskBridgeDbContext).Assembly);
    }

    /// <summary>Commits the current application unit of work.</summary>
    async Task IUnitOfWork.SaveChangesAsync(CancellationToken cancellationToken)
    {
        await SaveChangesAsync(cancellationToken);
    }
}