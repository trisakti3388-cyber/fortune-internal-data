using FortuneInternalData.Domain.Entities;
using FortuneInternalData.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace FortuneInternalData.Infrastructure.Persistence;

public class ApplicationDbContext : IdentityDbContext<IdentityApplicationUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    public DbSet<ApplicationUser> Users => Set<ApplicationUser>();
    public DbSet<PhoneNumberRecord> PhoneNumbers => Set<PhoneNumberRecord>();
    public DbSet<ImportBatch> ImportBatches => Set<ImportBatch>();
    public DbSet<ImportBatchRow> ImportBatchRows => Set<ImportBatchRow>();
    public DbSet<ActivityLog> ActivityLogs => Set<ActivityLog>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<ApplicationUser>(entity =>
        {
            entity.ToTable("users");
            entity.HasKey(x => x.Id);
            entity.HasIndex(x => x.Email).IsUnique();
            entity.Property(x => x.Name).HasMaxLength(100).IsRequired();
            entity.Property(x => x.Email).HasMaxLength(150).IsRequired();
            entity.Property(x => x.PasswordHash).HasMaxLength(255).IsRequired();
            entity.Property(x => x.Role).HasMaxLength(20).IsRequired();
            entity.Property(x => x.TwoFactorSecret).HasMaxLength(255);
        });

        modelBuilder.Entity<PhoneNumberRecord>(entity =>
        {
            entity.ToTable("phone_numbers");
            entity.HasKey(x => x.Id);
            entity.HasIndex(x => x.PhoneNumber).IsUnique();
            entity.Property(x => x.Seq).HasMaxLength(50);
            entity.Property(x => x.PhoneNumber).HasMaxLength(14).IsRequired();
            entity.Property(x => x.Status).HasMaxLength(20).IsRequired();
            entity.Property(x => x.WhatsappStatus).HasMaxLength(20);
        });

        modelBuilder.Entity<ImportBatch>(entity =>
        {
            entity.ToTable("import_batches");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.FileName).HasMaxLength(255).IsRequired();
            entity.Property(x => x.StoredFilePath).HasMaxLength(255);
            entity.Property(x => x.Status).HasMaxLength(30).IsRequired();
        });

        modelBuilder.Entity<ImportBatchRow>(entity =>
        {
            entity.ToTable("import_batch_rows");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Seq).HasMaxLength(50);
            entity.Property(x => x.RawPhoneNumber).HasMaxLength(100);
            entity.Property(x => x.NormalizedPhoneNumber).HasMaxLength(14);
            entity.Property(x => x.RowStatus).HasMaxLength(30).IsRequired();
            entity.Property(x => x.Message).HasMaxLength(255);
        });

        modelBuilder.Entity<ActivityLog>(entity =>
        {
            entity.ToTable("activity_logs");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Action).HasMaxLength(50).IsRequired();
            entity.Property(x => x.TargetType).HasMaxLength(50).IsRequired();
        });
    }
}
