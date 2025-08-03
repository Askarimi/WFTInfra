using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WFT.Infra.Core.Entities.UserManagment;

namespace WFT.Infra.Infrastructure.Configurations
{
    public class ConditionOperatorConfiguration : IEntityTypeConfiguration<ConditionOperator>
    {
        public void Configure(EntityTypeBuilder<ConditionOperator> builder)
        {
            builder.ToTable("ConditionOperators");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Name)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(x => x.DisplayName)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(x => x.Symbol)
                .IsRequired()
                .HasMaxLength(20);

            builder.Property(x => x.DataTypes)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(x => x.IsActive)
                .IsRequired()
                .HasDefaultValue(true);

            // Indexes
            builder.HasIndex(x => x.Name).IsUnique();
            builder.HasIndex(x => x.IsActive);

            // Relationships
            builder.HasMany(x => x.PolicyConditions)
                .WithOne(x => x.ConditionOperator)
                .HasForeignKey(x => x.ConditionOperatorId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
} 