using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WFT.Infra.Core.Entities.UserManagment;

namespace WFT.Infra.Infrastructure.Configurations
{
    public class RoleConfiguration : IEntityTypeConfiguration<Role>
    {
        public void Configure(EntityTypeBuilder<Role> builder)
        {
            builder.ToTable("Roles");
            
            builder.HasKey(r => r.Id);
            
            builder.Property(r => r.Name)
                .IsRequired()
                .HasMaxLength(100);
            
            builder.Property(r => r.Description)
                .HasMaxLength(500);
            
            builder.Property(r => r.IsActive)
                .IsRequired()
                .HasDefaultValue(true);
            
            // Index for better query performance
            builder.HasIndex(r => r.Name)
                .IsUnique();
            
            // One-to-Many relationship with UserRoles
            builder.HasMany(r => r.UserRoles)
                .WithOne(ur => ur.Role)
                .HasForeignKey(ur => ur.RoleId)
                .OnDelete(DeleteBehavior.Cascade);
            
            // One-to-Many relationship with RolePermissions
            builder.HasMany(r => r.RolePermissions)
                .WithOne(rp => rp.Role)
                .HasForeignKey(rp => rp.RoleId)
                .OnDelete(DeleteBehavior.Cascade);
            
            // One-to-Many relationship with RolePolicyRules
            builder.HasMany(r => r.RolePolicyRules)
                .WithOne(rpr => rpr.Role)
                .HasForeignKey(rpr => rpr.RoleId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}


