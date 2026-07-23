using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WFT.Infra.Core.Entities.UserManagment;

namespace WFT.Infra.Infrastructure.Configurations
{
    public class AttributeGroupConfiguration : IEntityTypeConfiguration<AttributeGroup>
    {
        public void Configure(EntityTypeBuilder<AttributeGroup> builder)
        {
            builder.ToTable("AttributeGroups");
            
            builder.HasKey(x => x.Id);
            
            builder.Property(x => x.Name)
                .IsRequired()
                .HasMaxLength(100);
                
            builder.Property(x => x.DisplayName)
                .IsRequired()
                .HasMaxLength(200);
                
            builder.Property(x => x.Description)
                .HasMaxLength(500);
                
            builder.Property(x => x.Icon)
                .HasMaxLength(50);
                
            builder.Property(x => x.Color)
                .HasMaxLength(20);
                
            builder.Property(x => x.IsActive)
                .IsRequired()
                .HasDefaultValue(true);
                
            builder.Property(x => x.SortOrder)
                .IsRequired()
                .HasDefaultValue(0);
            
            // Indexes
            builder.HasIndex(x => x.Name).IsUnique();
            builder.HasIndex(x => x.SortOrder);
            builder.HasIndex(x => x.IsActive);
            
            // Relationships
            builder.HasMany(x => x.AttributeDefinitions)
                .WithOne(x => x.AttributeGroup)
                .HasForeignKey(x => x.AttributeGroupId)
                .OnDelete(DeleteBehavior.SetNull); // Don't cascade delete attributes when group is deleted
        }
    }
} 