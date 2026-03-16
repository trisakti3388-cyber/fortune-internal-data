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

    public DbSet<PhoneNumberRecord> PhoneNumbers => Set<PhoneNumberRecord>();
    public DbSet<ImportBatch> ImportBatches => Set<ImportBatch>();
    public DbSet<ImportBatchRow> ImportBatchRows => Set<ImportBatchRow>();
    public DbSet<ActivityLog> ActivityLogs => Set<ActivityLog>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<PhoneNumberRecord>(entity =>
        {
            entity.ToTable("phone_numbers");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Id).ValueGeneratedOnAdd();
            entity.HasIndex(x => x.PhoneNumber).IsUnique();
            entity.Property(x => x.Seq).HasMaxLength(50);
            entity.Property(x => x.PhoneNumber).HasColumnName("phone_number").HasMaxLength(14).IsRequired();
            entity.Property(x => x.Status).HasColumnName("status").HasMaxLength(20).IsRequired().HasDefaultValue("active");
            entity.Property(x => x.WhatsappStatus).HasColumnName("whatsapp_status").HasMaxLength(20);
            entity.Property(x => x.Remark).HasColumnName("remark");
            entity.Property(x => x.UploadDate).HasColumnName("upload_date");
            entity.Property(x => x.ModifiedDate).HasColumnName("modified_date");
            entity.Property(x => x.CreatedAt).HasColumnName("created_at");
            entity.Property(x => x.UpdatedAt).HasColumnName("updated_at");
        });

        modelBuilder.Entity<ImportBatch>(entity =>
        {
            entity.ToTable("import_batches");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Id).ValueGeneratedOnAdd();
            entity.Property(x => x.FileName).HasColumnName("file_name").HasMaxLength(255).IsRequired();
            entity.Property(x => x.StoredFilePath).HasColumnName("stored_file_path").HasMaxLength(255);
            entity.Property(x => x.UploadedByUserId).HasColumnName("uploaded_by_user_id").HasMaxLength(450).IsRequired();
            entity.Property(x => x.Status).HasColumnName("status").HasMaxLength(30).IsRequired();
            entity.Property(x => x.TotalRows).HasColumnName("total_rows");
            entity.Property(x => x.NewRows).HasColumnName("new_rows");
            entity.Property(x => x.ExistingRows).HasColumnName("existing_rows");
            entity.Property(x => x.InvalidRows).HasColumnName("invalid_rows");
            entity.Property(x => x.DuplicateRows).HasColumnName("duplicate_rows");
            entity.Property(x => x.ProcessedRows).HasColumnName("processed_rows").HasDefaultValue(0);
            entity.Property(x => x.ErrorMessage).HasColumnName("error_message").HasMaxLength(1000);
            entity.Property(x => x.CreatedAt).HasColumnName("created_at");
            entity.Property(x => x.UpdatedAt).HasColumnName("updated_at");
        });

        modelBuilder.Entity<ImportBatchRow>(entity =>
        {
            entity.ToTable("import_batch_rows");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Id).ValueGeneratedOnAdd();
            entity.Property(x => x.BatchId).HasColumnName("batch_id");
            entity.Property(x => x.Seq).HasColumnName("seq").HasMaxLength(50);
            entity.Property(x => x.RawPhoneNumber).HasColumnName("raw_phone_number").HasMaxLength(100);
            entity.Property(x => x.NormalizedPhoneNumber).HasColumnName("normalized_phone_number").HasMaxLength(14);
            entity.Property(x => x.Remark).HasColumnName("remark");
            entity.Property(x => x.RowStatus).HasColumnName("row_status").HasMaxLength(30).IsRequired();
            entity.Property(x => x.Message).HasColumnName("message").HasMaxLength(255);
            entity.Property(x => x.CreatedAt).HasColumnName("created_at");
            entity.Property(x => x.UpdatedAt).HasColumnName("updated_at");

            entity.HasIndex(x => x.BatchId);
            entity.HasIndex(x => x.RowStatus);
            entity.HasIndex(x => x.NormalizedPhoneNumber);
        });

        modelBuilder.Entity<ActivityLog>(entity =>
        {
            entity.ToTable("activity_logs");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Id).ValueGeneratedOnAdd();
            entity.Property(x => x.UserId).HasColumnName("user_id").HasMaxLength(450).IsRequired();
            entity.Property(x => x.Action).HasColumnName("action").HasMaxLength(50).IsRequired();
            entity.Property(x => x.TargetType).HasColumnName("target_type").HasMaxLength(50).IsRequired();
            entity.Property(x => x.TargetId).HasColumnName("target_id");
            entity.Property(x => x.OldValueJson).HasColumnName("old_value_json").HasColumnType("json");
            entity.Property(x => x.NewValueJson).HasColumnName("new_value_json").HasColumnType("json");
            entity.Property(x => x.CreatedAt).HasColumnName("created_at");

            entity.HasIndex(x => x.UserId);
            entity.HasIndex(x => x.Action);
            entity.HasIndex(x => x.CreatedAt);
            entity.HasIndex(x => new { x.TargetType, x.TargetId });
        });
    }
}
