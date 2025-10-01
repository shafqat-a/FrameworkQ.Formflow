using Microsoft.EntityFrameworkCore;
using FormDesigner.API.Models.Entities;

namespace FormDesigner.API.Data;

/// <summary>
/// Entity Framework Core database context for the Form Designer application.
/// Manages form definitions (design mode) and form instances (runtime mode).
/// </summary>
public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    /// <summary>
    /// Form definitions (both draft and committed)
    /// </summary>
    public DbSet<FormDefinitionEntity> FormDefinitions { get; set; } = null!;

    /// <summary>
    /// Form instances for runtime data entry
    /// </summary>
    public DbSet<FormInstanceEntity> FormInstances { get; set; } = null!;

    /// <summary>
    /// Temporary states for saved progress during data entry
    /// </summary>
    public DbSet<TemporaryStateEntity> TemporaryStates { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure FormDefinitionEntity
        modelBuilder.Entity<FormDefinitionEntity>(entity =>
        {
            entity.ToTable("form_definitions");

            entity.HasKey(e => e.FormId);

            entity.Property(e => e.FormId)
                .HasColumnName("form_id")
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(e => e.Version)
                .HasColumnName("version")
                .IsRequired()
                .HasMaxLength(50);

            // Store as JSONB in PostgreSQL
            entity.Property(e => e.DslJson)
                .HasColumnName("dsl_json")
                .HasColumnType("jsonb")
                .IsRequired();

            entity.Property(e => e.IsCommitted)
                .HasColumnName("is_committed")
                .IsRequired()
                .HasDefaultValue(false);

            entity.Property(e => e.CreatedAt)
                .HasColumnName("created_at")
                .IsRequired()
                .HasDefaultValueSql("NOW()");

            entity.Property(e => e.UpdatedAt)
                .HasColumnName("updated_at");

            entity.Property(e => e.IsActive)
                .HasColumnName("is_active")
                .IsRequired()
                .HasDefaultValue(true);

            // Indexes
            entity.HasIndex(e => e.IsActive)
                .HasDatabaseName("ix_form_definitions_is_active");

            entity.HasIndex(e => e.IsCommitted)
                .HasDatabaseName("ix_form_definitions_is_committed");

            entity.HasIndex(e => e.CreatedAt)
                .HasDatabaseName("ix_form_definitions_created_at");

            // GIN index on JSONB for efficient querying
            entity.HasIndex(e => e.DslJson)
                .HasDatabaseName("ix_form_definitions_dsl_json")
                .HasMethod("gin");
        });

        // Configure FormInstanceEntity
        modelBuilder.Entity<FormInstanceEntity>(entity =>
        {
            entity.ToTable("form_instances");

            entity.HasKey(e => e.InstanceId);

            entity.Property(e => e.InstanceId)
                .HasColumnName("instance_id")
                .IsRequired();

            entity.Property(e => e.FormId)
                .HasColumnName("form_id")
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(e => e.Status)
                .HasColumnName("status")
                .IsRequired()
                .HasMaxLength(20)
                .HasDefaultValue("draft");

            // Store as JSONB in PostgreSQL
            entity.Property(e => e.DataJson)
                .HasColumnName("data_json")
                .HasColumnType("jsonb");

            entity.Property(e => e.CreatedAt)
                .HasColumnName("created_at")
                .IsRequired()
                .HasDefaultValueSql("NOW()");

            entity.Property(e => e.SubmittedAt)
                .HasColumnName("submitted_at");

            entity.Property(e => e.UserId)
                .HasColumnName("user_id")
                .HasMaxLength(256);

            // Foreign key relationship
            entity.HasOne(e => e.FormDefinition)
                .WithMany()
                .HasForeignKey(e => e.FormId)
                .OnDelete(DeleteBehavior.Restrict);

            // Indexes
            entity.HasIndex(e => e.FormId)
                .HasDatabaseName("ix_form_instances_form_id");

            entity.HasIndex(e => e.Status)
                .HasDatabaseName("ix_form_instances_status");

            entity.HasIndex(e => e.UserId)
                .HasDatabaseName("ix_form_instances_user_id");

            entity.HasIndex(e => e.CreatedAt)
                .HasDatabaseName("ix_form_instances_created_at");

            // GIN index on JSONB
            entity.HasIndex(e => e.DataJson)
                .HasDatabaseName("ix_form_instances_data_json")
                .HasMethod("gin");
        });

        // Configure TemporaryStateEntity
        modelBuilder.Entity<TemporaryStateEntity>(entity =>
        {
            entity.ToTable("temporary_states");

            entity.HasKey(e => e.StateId);

            entity.Property(e => e.StateId)
                .HasColumnName("state_id")
                .IsRequired();

            entity.Property(e => e.InstanceId)
                .HasColumnName("instance_id")
                .IsRequired();

            // Store as JSONB in PostgreSQL
            entity.Property(e => e.DataJson)
                .HasColumnName("data_json")
                .HasColumnType("jsonb")
                .IsRequired();

            entity.Property(e => e.SavedAt)
                .HasColumnName("saved_at")
                .IsRequired()
                .HasDefaultValueSql("NOW()");

            entity.Property(e => e.UserId)
                .HasColumnName("user_id")
                .HasMaxLength(256);

            // Foreign key relationship
            entity.HasOne(e => e.FormInstance)
                .WithMany()
                .HasForeignKey(e => e.InstanceId)
                .OnDelete(DeleteBehavior.Cascade);

            // Indexes
            entity.HasIndex(e => e.InstanceId)
                .HasDatabaseName("ix_temporary_states_instance_id");

            entity.HasIndex(e => e.SavedAt)
                .HasDatabaseName("ix_temporary_states_saved_at");

            entity.HasIndex(e => e.UserId)
                .HasDatabaseName("ix_temporary_states_user_id");
        });
    }
}
