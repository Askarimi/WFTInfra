using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WFT.Infra.Core.Entities.UserManagment;

namespace WFT.Infra.Infrastructure.Configurations
{
    public class AttributeDefinitionConfiguration : IEntityTypeConfiguration<AttributeDefinition>
    {
        public void Configure(EntityTypeBuilder<AttributeDefinition> builder)
        {
            builder.ToTable("AttributeDefinitions");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Name)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(x => x.DisplayName)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(x => x.DataType)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(x => x.Source)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(x => x.DefaultValue)
                .HasMaxLength(500);

            builder.Property(x => x.Description)
                .HasMaxLength(1000);

            builder.Property(x => x.IsActive)
                .IsRequired()
                .HasDefaultValue(true);

            // Indexes
            builder.HasIndex(x => x.Name).IsUnique();
            builder.HasIndex(x => x.Source);
            builder.HasIndex(x => x.IsActive);

            // Relationships
            builder.HasMany(x => x.AttributeValues)
                .WithOne(x => x.AttributeDefinition)
                .HasForeignKey(x => x.AttributeDefinitionId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(x => x.PolicyConditions)
                .WithOne(x => x.AttributeDefinition)
                .HasForeignKey(x => x.AttributeDefinitionId)
                .OnDelete(DeleteBehavior.Restrict);

            // Group relationship
            builder.HasOne(x => x.AttributeGroup)
                .WithMany(x => x.AttributeDefinitions)
                .HasForeignKey(x => x.AttributeGroupId)
                .OnDelete(DeleteBehavior.SetNull);

            builder.HasIndex(x => x.AttributeGroupId);
        }
    }
} 