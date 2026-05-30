using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Pivot.Models.Entities;

namespace Pivot.Data.Configurations;

public class MemberDetailConfiguration : IEntityTypeConfiguration<MemberDetail>
{
    public void Configure(EntityTypeBuilder<MemberDetail> builder)
    {
        builder.ToTable("MemberDetails");
        builder.HasKey(m => m.Id);

        builder.Property(m => m.FirstName).IsRequired().HasMaxLength(100);
        builder.Property(m => m.LastName).IsRequired().HasMaxLength(100);
        builder.Property(m => m.PersonalId).HasMaxLength(50);
        builder.Property(m => m.Nationality).IsRequired().HasMaxLength(100);
        builder.Property(m => m.Birthday).IsRequired();
        builder.Property(m => m.Email).HasMaxLength(200);
        builder.Property(m => m.PhoneNumber).HasMaxLength(30);

        // Unique + nullable: MySQL birden çok NULL kabul eder, dolular unique olmak zorunda.
        builder.HasIndex(m => m.Email).IsUnique();
        builder.HasIndex(m => m.PhoneNumber).IsUnique();
    }
}
