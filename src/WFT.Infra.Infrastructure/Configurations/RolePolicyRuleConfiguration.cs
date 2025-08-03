using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WFT.Infra.Core.Entities.UserManagment;

namespace WFT.Infra.Infrastructure.Configurations
{
    public class RolePolicyRuleConfiguration : IEntityTypeConfiguration<RolePolicyRule>
    {
        public void Configure(EntityTypeBuilder<RolePolicyRule> builder)
        {
            builder.ToTable("RolePolicyRules");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.RoleId)
                .IsRequired();

            builder.Property(x => x.PolicyRuleId)
                .IsRequired();

            builder.Property(x => x.IsActive)
                .IsRequired()
                .HasDefaultValue(true);

            builder.Property(x => x.ValidFrom);

            builder.Property(x => x.ValidTo);

            // Indexes
            builder.HasIndex(x => x.RoleId);
            builder.HasIndex(x => x.PolicyRuleId);
            builder.HasIndex(x => x.IsActive);
            builder.HasIndex(x => x.ValidFrom);
            builder.HasIndex(x => x.ValidTo);
            builder.HasIndex(x => new { x.RoleId, x.PolicyRuleId }).IsUnique();

            // Relationships
            builder.HasOne(x => x.Role)
                .WithMany(x => x.RolePolicyRules)
                .HasForeignKey(x => x.RoleId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(x => x.PolicyRule)
                .WithMany(x => x.RolePolicyRules)
                .HasForeignKey(x => x.PolicyRuleId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
} 