using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WFT.Infra.Core.Entities.UserManagment;

namespace WFT.Infra.Infrastructure.Configurations
{
    public class PolicyConditionConfiguration : IEntityTypeConfiguration<PolicyCondition>
    {
        public void Configure(EntityTypeBuilder<PolicyCondition> builder)
        {
            builder.ToTable("PolicyConditions");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.PolicyRuleId)
                .IsRequired();

            builder.Property(x => x.AttributeDefinitionId)
                .IsRequired();

            builder.Property(x => x.ConditionOperatorId)
                .IsRequired();

            builder.Property(x => x.Value)
                .IsRequired()
                .HasMaxLength(500);

            builder.Property(x => x.LogicalOperator)
                .IsRequired()
                .HasMaxLength(10)
                .HasDefaultValue("AND")
                .HasConversion(
                    v => v.ToUpper(),
                    v => v);

            builder.Property(x => x.Order)
                .IsRequired()
                .HasDefaultValue(1);

            // Indexes
            builder.HasIndex(x => x.PolicyRuleId);
            builder.HasIndex(x => x.AttributeDefinitionId);
            builder.HasIndex(x => x.ConditionOperatorId);
            builder.HasIndex(x => x.Order);

            // Relationships
            builder.HasOne(x => x.PolicyRule)
                .WithMany(x => x.PolicyConditions)
                .HasForeignKey(x => x.PolicyRuleId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(x => x.AttributeDefinition)
                .WithMany(x => x.PolicyConditions)
                .HasForeignKey(x => x.AttributeDefinitionId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.ConditionOperator)
                .WithMany(x => x.PolicyConditions)
                .HasForeignKey(x => x.ConditionOperatorId)
                .OnDelete(DeleteBehavior.Restrict);

            // Check constraint: LogicalOperator must be either "AND" or "OR"
            builder.ToTable(t => t.HasCheckConstraint("CK_PolicyCondition_LogicalOperator", "LogicalOperator IN ('AND', 'OR')"));
        }
    }
} 