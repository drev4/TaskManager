using Microsoft.EntityFrameworkCore;
using TaskMgr.Api.Domain.Entities;
using TaskMgr.Api.Domain.Enums;

namespace TaskMgr.Api.Data;

/// <summary>
/// Entity Framework DbContext for TaskManager application
/// </summary>
public class TaskMgrDbContext : DbContext
{
    public TaskMgrDbContext(DbContextOptions<TaskMgrDbContext> options) : base(options) { }

    /// <summary>
    /// Users table
    /// </summary>
    public DbSet<User> Users { get; set; } = null!;
    
    /// <summary>
    /// Projects table
    /// </summary>
    public DbSet<Project> Projects { get; set; } = null!;
    
    /// <summary>
    /// Tasks table
    /// </summary>
    public DbSet<TaskItem> Tasks { get; set; } = null!;

    /// <summary>
    /// Configures the database schema
    /// </summary>
    /// <param name="modelBuilder">Model builder</param>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure User entity
        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("Users");
            entity.HasKey(e => e.Id);
            
            entity.Property(e => e.Email)
                .IsRequired()
                .HasMaxLength(255)
                .HasAnnotation("Description", "User's email address");
            
            entity.Property(e => e.DisplayName)
                .IsRequired()
                .HasMaxLength(100)
                .HasAnnotation("Description", "User's display name");
            
            entity.Property(e => e.AzureObjectId)
                .IsRequired()
                .HasMaxLength(100)
                .HasAnnotation("Description", "Azure AD B2C Object ID");
            
            entity.Property(e => e.Avatar)
                .HasMaxLength(500)
                .HasAnnotation("Description", "Profile picture URL");
            
            entity.Property(e => e.PreferredLanguage)
                .HasMaxLength(10)
                .HasAnnotation("Description", "User's preferred language/locale");
            
            entity.Property(e => e.CreatedAt)
                .IsRequired()
                .HasAnnotation("Description", "Date when user was created");
            
            entity.Property(e => e.UpdatedAt)
                .IsRequired()
                .HasAnnotation("Description", "Date when user was last updated");
            
            entity.Property(e => e.LastLoginAt)
                .HasAnnotation("Description", "Date when user last logged in");
            
            // Indexes
            entity.HasIndex(e => e.Email)
                .IsUnique()
                .HasDatabaseName("IX_Users_Email");
            
            entity.HasIndex(e => e.AzureObjectId)
                .IsUnique()
                .HasDatabaseName("IX_Users_AzureObjectId");
            
            // Relationships
            entity.HasMany(e => e.OwnedProjects)
                .WithOne(e => e.Owner)
                .HasForeignKey(e => e.OwnerUserId)
                .OnDelete(DeleteBehavior.Restrict);
            
            entity.HasMany(e => e.AssignedTasks)
                .WithOne(e => e.AssignedTo)
                .HasForeignKey(e => e.AssignedToUserId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        // Configure Project entity
        modelBuilder.Entity<Project>(entity =>
        {
            entity.ToTable("Projects");
            entity.HasKey(e => e.Id);
            
            entity.Property(e => e.Name)
                .IsRequired()
                .HasMaxLength(200)
                .HasAnnotation("Description", "Project name");
            
            entity.Property(e => e.Description)
                .HasMaxLength(1000)
                .HasAnnotation("Description", "Project description");
            
            entity.Property(e => e.Color)
                .HasMaxLength(7)
                .HasAnnotation("Description", "Project color for UI customization");
            
            entity.Property(e => e.CreatedAt)
                .IsRequired()
                .HasAnnotation("Description", "Date when project was created");
            
            entity.Property(e => e.UpdatedAt)
                .IsRequired()
                .HasAnnotation("Description", "Date when project was last updated");
            
            entity.Property(e => e.DueDate)
                .HasAnnotation("Description", "Project deadline");
            
            entity.Property(e => e.CompletedAt)
                .HasAnnotation("Description", "Date when project was completed");
            
            // Indexes
            entity.HasIndex(e => e.OwnerUserId)
                .HasDatabaseName("IX_Projects_OwnerUserId");
            
            entity.HasIndex(e => e.IsArchived)
                .HasDatabaseName("IX_Projects_IsArchived");
            
            // Relationships
            entity.HasOne(e => e.Owner)
                .WithMany(e => e.OwnedProjects)
                .HasForeignKey(e => e.OwnerUserId)
                .OnDelete(DeleteBehavior.Restrict);
            
            entity.HasMany(e => e.Tasks)
                .WithOne(e => e.Project)
                .HasForeignKey(e => e.ProjectId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Configure TaskItem entity
        modelBuilder.Entity<TaskItem>(entity =>
        {
            entity.ToTable("Tasks");
            entity.HasKey(e => e.Id);
            
            entity.Property(e => e.Title)
                .IsRequired()
                .HasMaxLength(300)
                .HasAnnotation("Description", "Task title");
            
            entity.Property(e => e.Description)
                .HasMaxLength(2000)
                .HasAnnotation("Description", "Task description");
            
            entity.Property(e => e.Status)
                .IsRequired()
                .HasConversion<int>()
                .HasAnnotation("Description", "Current status of the task");
            
            entity.Property(e => e.Priority)
                .IsRequired()
                .HasConversion<int>()
                .HasAnnotation("Description", "Priority level of the task");
            
            entity.Property(e => e.EstimatedHours)
                .HasPrecision(5, 2)
                .HasAnnotation("Description", "Estimated time to complete in hours");
            
            entity.Property(e => e.ActualHours)
                .HasPrecision(5, 2)
                .HasAnnotation("Description", "Actual time spent in hours");
            
            entity.Property(e => e.Tags)
                .HasMaxLength(500)
                .HasAnnotation("Description", "Comma-separated tags");
            
            entity.Property(e => e.CreatedAt)
                .IsRequired()
                .HasAnnotation("Description", "Date when task was created");
            
            entity.Property(e => e.UpdatedAt)
                .IsRequired()
                .HasAnnotation("Description", "Date when task was last updated");
            
            entity.Property(e => e.DueDate)
                .HasAnnotation("Description", "Task due date");
            
            entity.Property(e => e.CompletedAt)
                .HasAnnotation("Description", "Date when task was completed");
            
            // Indexes
            entity.HasIndex(e => e.ProjectId)
                .HasDatabaseName("IX_Tasks_ProjectId");
            
            entity.HasIndex(e => e.AssignedToUserId)
                .HasDatabaseName("IX_Tasks_AssignedToUserId");
            
            entity.HasIndex(e => e.Status)
                .HasDatabaseName("IX_Tasks_Status");
            
            entity.HasIndex(e => e.Priority)
                .HasDatabaseName("IX_Tasks_Priority");
            
            entity.HasIndex(e => e.DueDate)
                .HasDatabaseName("IX_Tasks_DueDate");
            
            // Relationships
            entity.HasOne(e => e.Project)
                .WithMany(e => e.Tasks)
                .HasForeignKey(e => e.ProjectId)
                .OnDelete(DeleteBehavior.Cascade);
            
            entity.HasOne(e => e.AssignedTo)
                .WithMany(e => e.AssignedTasks)
                .HasForeignKey(e => e.AssignedToUserId)
                .OnDelete(DeleteBehavior.SetNull);
        });
    }

    /// <summary>
    /// Override SaveChanges to automatically set audit fields
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Number of entities affected</returns>
    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        UpdateAuditFields();
        return await base.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// Override SaveChanges to automatically set audit fields
    /// </summary>
    /// <returns>Number of entities affected</returns>
    public override int SaveChanges()
    {
        UpdateAuditFields();
        return base.SaveChanges();
    }

    /// <summary>
    /// Updates audit fields for tracked entities
    /// </summary>
    private void UpdateAuditFields()
    {
        var entries = ChangeTracker.Entries<BaseEntity>();

        foreach (var entry in entries)
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    entry.Entity.SetCreated();
                    break;
                case EntityState.Modified:
                    entry.Entity.SetUpdated();
                    break;
            }
        }
    }
}