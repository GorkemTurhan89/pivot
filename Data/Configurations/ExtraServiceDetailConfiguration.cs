using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Pivot.Models.Entities;

namespace Pivot.Data.Configurations;

public class ExtraServiceDetailConfiguration : IEntityTypeConfiguration<ExtraServiceDetail>
{
    public void Configure(EntityTypeBuilder<ExtraServiceDetail> builder)
    {
        builder.ToTable("ExtraServiceDetails");
        builder.HasKey(d => d.PaymentPlanId);

        builder.Property(d => d.Country).IsRequired().HasMaxLength(100);
        builder.Property(d => d.VisaType).HasMaxLength(100);
        builder.Property(d => d.DefaultPriceText).IsRequired().HasMaxLength(100);
        builder.Property(d => d.Currency).IsRequired().HasMaxLength(3);

        builder.HasOne(d => d.PaymentPlan)
            .WithOne()
            .HasForeignKey<ExtraServiceDetail>(d => d.PaymentPlanId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(d => d.Country);
    }
}
