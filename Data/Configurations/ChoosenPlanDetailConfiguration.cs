using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Pivot.Models.Entities;

namespace Pivot.Data.Configurations;

public class ChoosenPlanDetailConfiguration : IEntityTypeConfiguration<ChoosenPlanDetail>
{
    public void Configure(EntityTypeBuilder<ChoosenPlanDetail> builder)
    {
        builder.ToTable("ChoosenPlanDetails");
        builder.HasKey(p => p.Id);

        builder.Property(p => p.Name).IsRequired().HasMaxLength(200);
        builder.Property(p => p.Category).HasMaxLength(100);
        builder.Property(p => p.PriceType).IsRequired().HasConversion<int>();
        builder.Property(p => p.Currency).IsRequired().HasMaxLength(3);

        builder.Property(p => p.WeeklyListFee).HasPrecision(18, 2);
        builder.Property(p => p.WeeklyPromoFee).HasPrecision(18, 2);
        builder.Property(p => p.TotalListFee).HasPrecision(18, 2);
        builder.Property(p => p.TotalPromoFee).HasPrecision(18, 2);
        builder.Property(p => p.LineAmount).HasPrecision(18, 2);
        builder.Property(p => p.RegistrationFee).HasPrecision(18, 2);

        builder.HasOne(p => p.CartDetail)
            .WithMany(c => c.ChoosenPlans)
            .HasForeignKey(p => p.CartDetailId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(p => p.CartDetailId);
    }
}
