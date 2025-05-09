using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RPK.Infra.Core.Entities;

namespace RPK.Infra.Infrastructure.Configurations
{
    public class SampleEntityConfiguration : IEntityTypeConfiguration<SampleEntity>
    {
        public void Configure(EntityTypeBuilder<SampleEntity> builder)
        {
            builder.ToTable("SampleEntities");

            builder.HasKey(e => e.Id);

            builder.Property(e => e.Name)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(e => e.Description)
                .IsRequired()
                .HasMaxLength(500);

            builder.Property(e => e.IsActive)
                .IsRequired()
                .HasDefaultValue(true);

            builder.Property(e => e.CreatedAt)
                .IsRequired();

            builder.Property(e => e.UpdatedAt);
        }
    }
} 