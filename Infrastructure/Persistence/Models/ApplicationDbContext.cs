using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Domain.Entities;

namespace Infrastructure.Persistence.Models;

public partial class ApplicationDbContext : DbContext
{
    public ApplicationDbContext()
    {
    }

    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<User> Users { get; set; }
    public virtual DbSet<PasskeyCredential> PasskeyCredentials { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("users_pkey");

            entity.ToTable("users", "core");

            entity.HasIndex(e => e.Email, "users_email_key").IsUnique();

            entity.Property(e => e.Id)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("id");
            // Note: CreatedAt and UpdatedAt are not in Domain.User, 
            // so we'll map them as shadow properties if needed or just skip them for now.
            // But they were in the previous mapping. 
            // I'll add them to Domain.User to be safe.
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnName("created_at");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnName("updated_at");
            entity.Property(e => e.Email)
                .HasMaxLength(255)
                .HasColumnName("email");
            entity.Property(e => e.IsVerified)
                .HasDefaultValue(false)
                .HasColumnName("is_verified");
            entity.Property(e => e.PasswordHash)
                .HasMaxLength(255)
                .HasColumnName("password_hash");
            entity.Property(e => e.TotpSecret)
                .HasMaxLength(64)
                .HasColumnName("totp_secret");
        });

        modelBuilder.Entity<PasskeyCredential>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("fido_credentials_pkey");
            entity.ToTable("fido_credentials", "core");

            entity.Property(e => e.Id).HasDefaultValueSql("gen_random_uuid()").HasColumnName("id");
            entity.Property(e => e.UserId).HasColumnName("user_id");
            entity.Property(e => e.DescriptorId).HasColumnName("descriptor_id");
            entity.Property(e => e.PublicKey).HasColumnName("public_key");
            entity.Property(e => e.UserHandle).HasColumnName("user_handle");
            entity.Property(e => e.SignatureCounter).HasColumnName("signature_counter");
            entity.Property(e => e.CredType).HasColumnName("cred_type");
            entity.Property(e => e.RegDate).HasColumnName("reg_date");
            entity.Property(e => e.AaGuid).HasColumnName("aa_guid");

            entity.HasOne(d => d.User)
                .WithMany(p => p.Credentials)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("fido_credentials_user_id_fkey");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
