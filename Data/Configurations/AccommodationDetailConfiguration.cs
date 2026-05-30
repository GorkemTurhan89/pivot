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

        builder.HasOne(d => d.PaymentPlan)
            .WithOne()
            .HasForeignKey<AccommodationDetail>(d => d.PaymentPlanId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
