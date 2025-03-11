using Hangfire.Dashboard.Blazor.Core.Models;
using Microsoft.EntityFrameworkCore;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace Hangfire.Dashboard.Blazor.Postgresql.Context;

public partial class HangfireContext : DbContext
{
    public HangfireContext()
    {
    }

    public HangfireContext(DbContextOptions<HangfireContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Job> Jobs { get; set; }

    public virtual DbSet<Jobparameter> Jobparameters { get; set; }

    public virtual DbSet<Jobqueue> Jobqueues { get; set; }

    public virtual DbSet<State> States { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https: //go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseNpgsql(
            "Host=localhost;Port=5432;Database=sminex_comfort;Username=postgres;Password=postgres;Include Error Detail=true");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Job>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("job_pkey");

            entity.ToTable("job", "hangfire-etl-data");

            entity.HasIndex(e => e.Expireat, "ix_hangfire_job_expireat");

            entity.HasIndex(e => e.State, "ix_hangfire_job_statename");

            entity.HasIndex(e => e.State, "ix_hangfire_job_statename_is_not_null").HasFilter("(statename IS NOT NULL)");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Arguments)
                .HasColumnType("jsonb")
                .HasColumnName("arguments");
            entity.Property(e => e.Createdat).HasColumnName("createdat");
            entity.Property(e => e.Expireat).HasColumnName("expireat");
            entity.Property(e => e.Invocation)
                .HasColumnType("jsonb")
                .HasColumnName("invocationdata");

            entity.Property(e => e.State)
                .HasColumnName("statename")
                .HasComputedColumnSql(@"""""");
        });

        modelBuilder.Entity<Jobparameter>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("jobparameter_pkey");

            entity.ToTable("jobparameter", "hangfire-etl-data");

            entity.HasIndex(e => new { e.Jobid, e.Name }, "ix_hangfire_jobparameter_jobidandname");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Jobid).HasColumnName("jobid");
            entity.Property(e => e.Name).HasColumnName("name");
            
            entity.Property(e => e.Value).HasColumnName("value");
        });

        modelBuilder.Entity<Jobqueue>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("jobqueue_pkey");

            entity.ToTable("jobqueue", "hangfire-etl-data");

            entity.HasIndex(e => new { e.Fetchedat, e.Queue, e.Jobid }, "ix_hangfire_jobqueue_fetchedat_queue_jobid")
                .HasNullSortOrder(new[] { NullSortOrder.NullsFirst, NullSortOrder.NullsLast, NullSortOrder.NullsLast });

            entity.HasIndex(e => new { e.Jobid, e.Queue }, "ix_hangfire_jobqueue_jobidandqueue");

            entity.HasIndex(e => new { e.Queue, e.Fetchedat }, "ix_hangfire_jobqueue_queueandfetchedat");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Fetchedat).HasColumnName("fetchedat");
            entity.Property(e => e.Jobid).HasColumnName("jobid");
            entity.Property(e => e.Queue).HasColumnName("queue");
            entity.Property(e => e.Updatecount)
                .HasDefaultValue(0)
                .HasColumnName("updatecount");
        });

        modelBuilder.Entity<State>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("state_pkey");

            entity.ToTable("state", "hangfire-etl-data");

            entity.HasIndex(e => e.Jobid, "ix_hangfire_state_jobid");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Createdat).HasColumnName("createdat");
            entity.Property(e => e.Data)
                .HasColumnType("jsonb")
                .HasColumnName("data");
            entity.Property(e => e.Jobid).HasColumnName("jobid");
            entity.Property(e => e.Name).HasColumnName("name");
            entity.Property(e => e.Reason).HasColumnName("reason");
            entity.Property(e => e.Updatecount)
                .HasDefaultValue(0)
                .HasColumnName("updatecount");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}