using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WFT.Infra.Core.Entities.UserManagment;

namespace WFT.Infra.Infrastructure.Configurations
{
    public class AttributeValueConfiguration : IEntityTypeConfiguration<AttributeValue>
    {
        public void Configure(EntityTypeBuilder<AttributeValue> builder)
        {
            builder.ToTable("AttributeValues");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.AttributeDefinitionId)
                .IsRequired();

            builder.Property(x => x.UserId);

            builder.Property(x => x.ResourceId);

            builder.Property(x => x.ResourceType)
                .HasMaxLength(100);

            builder.Property(x => x.Value)
                .IsRequired()
                .HasMaxLength(1000);

            builder.Property(x => x.ValidFrom);

            builder.Property(x => x.ValidTo);

            builder.Property(x => x.IsActive)
                .IsRequired()
                .HasDefaultValue(true);

            // Indexes
            builder.HasIndex(x => x.AttributeDefinitionId);
            builder.HasIndex(x => x.UserId);
            builder.HasIndex(x => new { x.ResourceId, x.ResourceType });
            builder.HasIndex(x => x.IsActive);
            builder.HasIndex(x => x.ValidFrom);
            builder.HasIndex(x => x.ValidTo);

            // Relationships
            builder.HasOne(x => x.AttributeDefinition)
                .WithMany(x => x.AttributeValues)
                .HasForeignKey(x => x.AttributeDefinitionId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(x => x.User)
                .WithMany()
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Check constraint: Either UserId or ResourceId must be set
            builder.ToTable(t => t.HasCheckConstraint("CK_AttributeValue_UserId_Or_ResourceId", 
                "(UserId IS NOT NULL AND ResourceId IS NULL) OR (UserId IS NULL AND ResourceId IS NOT NULL)"));
        }
    }
} 