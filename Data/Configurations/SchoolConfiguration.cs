using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Pivot.Models.Entities;

namespace Pivot.Data.Configurations;

public class SchoolConfiguration : IEntityTypeConfiguration<School>
{
    public void Configure(EntityTypeBuilder<School> builder)
    {
        builder.ToTable("Schools");

        builder.HasKey(s => s.Id);

        builder.Property(s => s.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.HasOne(s => s.City)
            .WithMany(c => c.Schools)
            .HasForeignKey(s => s.CityId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(s => new { s.CityId, s.Name }).IsUnique();
    }
}
