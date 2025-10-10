using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WFT.Infra.Core.Entities.UserManagment;

namespace WFT.Infra.Infrastructure.Configurations
{
    public class UserConfiguration : IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> builder)
        {
            builder.ToTable("Users");
            
            builder.HasKey(u => u.Id);
            
            builder.Property(u => u.Username)
                .IsRequired()
                .HasMaxLength(100);
            
            builder.Property(u => u.Email)
                .IsRequired()
                .HasMaxLength(256);
            
            builder.Property(u => u.FirstName)
                .HasMaxLength(100);
            
            builder.Property(u => u.LastName)
                .HasMaxLength(100);
            
            builder.Property(u => u.EmailConfirmed)
                .IsRequired()
                .HasDefaultValue(false);
            
            builder.Property(u => u.IsActive)
                .IsRequired()
                .HasDefaultValue(true);
            
            builder.Property(u => u.LastLoginAt)
                .IsRequired(false);
            
            // Indexes for better query performance
            builder.HasIndex(u => u.Username)
                .IsUnique();
            
            builder.HasIndex(u => u.Email)
                .IsUnique();
            
            // One-to-One relationship with UserPassword is configured in UserPasswordConfiguration
            
            // One-to-Many relationship with UserRoles
            builder.HasMany(u => u.UserRoles)
                .WithOne(ur => ur.User)
                .HasForeignKey(ur => ur.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}

