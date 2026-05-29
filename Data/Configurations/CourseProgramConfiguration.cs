using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Pivot.Models.Entities;

namespace Pivot.Data.Configurations;

public class CourseProgramConfiguration : IEntityTypeConfiguration<CourseProgram>
{
    public void Configure(EntityTypeBuilder<CourseProgram> builder)
    {
        builder.ToTable("Programs");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.Name)
            .IsRequired()
            .HasMaxLength(150);

        builder.HasOne(p => p.School)
            .WithMany(s => s.Programs)
            .HasForeignKey(p => p.SchoolId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(p => new { p.SchoolId, p.Name }).IsUnique();
    }
}
