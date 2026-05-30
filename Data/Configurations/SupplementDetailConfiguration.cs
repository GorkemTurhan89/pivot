using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Pivot.Models.Entities;

namespace Pivot.Data.Configurations;

public class SupplementDetailConfiguration : IEntityTypeConfiguration<SupplementDetail>
{
    public void Configure(EntityTypeBuilder<SupplementDetail> builder)
    {
        builder.ToTable("SupplementDetails");

        builder.HasKey(d => d.PaymentPlanId);

        builder.Property(d => d.School).IsRequired().HasMaxLength(50);
        builder.Property(d => d.Country).IsRequired().HasMaxLength(100);
        builder.Property(d => d.Campus).IsRequired().HasMaxLength(100);
        builder.Property(d => d.AppliesTo).IsRequired().HasMaxLength(50);
        builder.Property(d => d.SupplementName).IsRequired().HasMaxLength(150);
        builder.Property(d => d.Required).IsRequired().HasMaxLength(50);
        builder.Property(d => d.Notes).HasColumnType("text");

        builder.HasOne(d => d.PaymentPlan)
            .WithOne()
            .HasForeignKey<SupplementDetail>(d => d.PaymentPlanId)
            .OnDelete(DeleteBehavior.Cascade);

        // Eşleşme sorgusu için index.
        builder.HasIndex(d => new { d.School, d.Country, d.Campus, d.AppliesTo });
    }
}
