using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WFT.Infra.Core.Entities.UserManagment;

namespace WFT.Infra.Infrastructure.Configurations
{
    public class PolicyRuleConfiguration : IEntityTypeConfiguration<PolicyRule>
    {
        public void Configure(EntityTypeBuilder<PolicyRule> builder)
        {
            builder.ToTable("PolicyRules");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Name)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(x => x.DisplayName)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(x => x.Description)
                .HasMaxLength(1000);

            builder.Property(x => x.IsActive)
                .IsRequired()
                .HasDefaultValue(true);

            builder.Property(x => x.Priority)
                .IsRequired()
                .HasDefaultValue(1);

            builder.Property(x => x.Effect)
                .IsRequired()
                .HasMaxLength(10)
                .HasConversion(
                    v => v.ToUpper(),
                    v => v);

            // Indexes
            builder.HasIndex(x => x.Name).IsUnique();
            builder.HasIndex(x => x.IsActive);
            builder.HasIndex(x => x.Priority);
            builder.HasIndex(x => x.Effect);

            // Relationships
            builder.HasMany(x => x.PolicyConditions)
                .WithOne(x => x.PolicyRule)
                .HasForeignKey(x => x.PolicyRuleId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(x => x.RolePolicyRules)
                .WithOne(x => x.PolicyRule)
                .HasForeignKey(x => x.PolicyRuleId)
                .OnDelete(DeleteBehavior.Cascade);

            // Check constraint: Effect must be either "Allow" or "Deny"
            builder.HasCheckConstraint("CK_PolicyRule_Effect", "Effect IN ('ALLOW', 'DENY')");
        }
    }
} 