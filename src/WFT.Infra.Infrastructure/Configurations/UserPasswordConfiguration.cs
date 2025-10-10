using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WFT.Infra.Core.Entities.UserManagment;

namespace WFT.Infra.Infrastructure.Configurations
{
    public class UserPasswordConfiguration : IEntityTypeConfiguration<UserPassword>
    {
        public void Configure(EntityTypeBuilder<UserPassword> builder)
        {
            builder.ToTable("UserPasswords");
            
            builder.HasKey(up => up.Id);
            
            builder.Property(up => up.PasswordHash)
                .IsRequired()
                .HasMaxLength(500);
            
            builder.Property(up => up.LastChangedAt)
                .IsRequired(false);
            
            // Unique index on UserId (one password per user)
            builder.HasIndex(up => up.UserId)
                .IsUnique();
            
            // One-to-One relationship with User
            builder.HasOne(up => up.User)
                .WithOne(u => u.Password)
                .HasForeignKey<UserPassword>(up => up.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}

