using Hangfire.Dashboard.Blazor.Postgresql.Models;
using Hangfire.PostgreSql;
using Microsoft.EntityFrameworkCore;

namespace Hangfire.Dashboard.Blazor.Postgresql.Context;

public class HangfirePostgresqlContext : DbContext
{
    internal static string? SchemaName;

    public HangfirePostgresqlContext(DbContextOptions<HangfirePostgresqlContext> options, PostgreSqlStorageOptions postgreSqlStorageOptions)
        : base(options)
    {
        SchemaName = postgreSqlStorageOptions.SchemaName;
    }

    public virtual DbSet<Job> Jobs { get; set; }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        if (!string.IsNullOrWhiteSpace(SchemaName))
        {
            modelBuilder.HasDefaultSchema(SchemaName);
        }
        modelBuilder.Entity<Job>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("job_pkey");

            entity.ToTable("job", "hangfire");

            entity.HasIndex(e => e.Expireat, "ix_hangfire_job_expireat");

            entity.HasIndex(e => e.State, "ix_hangfire_job_statename");

            entity.HasIndex(e => e.State, "ix_hangfire_job_statename_is_not_null").HasFilter("(statename IS NOT NULL)");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Arguments)
                .HasColumnType("jsonb")
                .HasColumnName("job_args")
                .IsRequired(false);
            
            entity.Property(e => e.Createdat).HasColumnName("createdat");
            entity.Property(e => e.Expireat).HasColumnName("expireat");
            entity.Property(e => e.Invocation)
                .HasColumnType("jsonb")
                .HasColumnName("invocationdata");

            entity.Property(e => e.State)
                .HasColumnName("statename");
        });
    }
}