using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Pivot.Models.Entities;

namespace Pivot.Data.Configurations;

public class AccommodationDetailConfiguration : IEntityTypeConfiguration<AccommodationDetail>
{
    public void Configure(EntityTypeBuilder<AccommodationDetail> builder)
    {
        builder.ToTable("AccommodationDetails");

        // PaymentPlanId hem PK hem FK -> garantili 1:1 ilişki.
        builder.HasKey(d => d.PaymentPlanId);

        builder.Property(d => d.Type).IsRequired().HasMaxLength(50);
        builder.Property(d => d.RoomType).IsRequired().HasMaxLength(80);
        builder.Property(d => d.Board).IsRequired().HasMaxLength(50);
        builder.Property(d => d.ResidenceName).HasMaxLength(150);

        builder.Property(d => d.School).IsRequired().HasMaxLength(50);
        builder.Property(d => d.Country).IsRequired().HasMaxLength(100);
        builder.Property(d => d.Campus).IsRequired().HasMaxLength(100);
        builder.Property(d => d.Required).IsRequired().HasMaxLength(50);
        builder.Property(d => d.DisplayOption).IsRequired().HasMaxLength(300);

        // Supplement eşleşmesi sık yapılacak; compound index düşürmek isterse:
        builder.HasIndex(d => new { d.School, d.Country, d.Campus, d.Type });

        builder.HasOne(d => d.PaymentPlan)
            .WithOne()
            .HasForeignKey<AccommodationDetail>(d => d.PaymentPlanId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
