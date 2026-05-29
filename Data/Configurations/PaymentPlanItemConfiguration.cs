using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Pivot.Models.Entities;

namespace Pivot.Data.Configurations;

public class PaymentPlanItemConfiguration : IEntityTypeConfiguration<PaymentPlanItem>
{
    public void Configure(EntityTypeBuilder<PaymentPlanItem> builder)
    {
        builder.ToTable("PaymentPlanItems");

        builder.HasKey(i => i.Id);

        builder.Property(i => i.ItemName)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(i => i.ItemPrice)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(i => i.Currency)
            .IsRequired()
            .HasMaxLength(3);

        builder.Property(i => i.Quantity)
            .HasDefaultValue(1);

        builder.HasOne(i => i.PaymentPlan)
            .WithMany(p => p.Items)
            .HasForeignKey(i => i.PaymentPlanId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
