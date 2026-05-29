using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Pivot.Models.Entities;

namespace Pivot.Data.Configurations;

public class PaymentPlanConfiguration : IEntityTypeConfiguration<PaymentPlan>
{
    public void Configure(EntityTypeBuilder<PaymentPlan> builder)
    {
        builder.ToTable("PaymentPlans");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(p => p.LengthWeeks)
            .IsRequired();

        builder.Property(p => p.ValidFrom).IsRequired();
        builder.Property(p => p.ValidTo).IsRequired();

        builder.Property(p => p.PackageType)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(p => p.IsAdditional)
            .HasDefaultValue(false);

        builder.Property(p => p.Category)
            .HasMaxLength(100);

        builder.Property(p => p.MembershipFee).HasPrecision(18, 2);
        builder.Property(p => p.RegistrationFee).HasPrecision(18, 2);
        builder.Property(p => p.PromotedFee).HasPrecision(18, 2);

        builder.Property(p => p.IsOrHasMandatory)
            .HasDefaultValue(false);

        builder.Property(p => p.IsActive)
            .HasDefaultValue(true);

        builder.HasOne(p => p.School)
            .WithMany(s => s.PaymentPlans)
            .HasForeignKey(p => p.SchoolId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(p => p.Program)
            .WithMany(pr => pr.PaymentPlans)
            .HasForeignKey(p => p.ProgramId)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired(false);

        builder.HasIndex(p => new { p.SchoolId, p.ProgramId, p.LengthWeeks, p.ValidFrom, p.ValidTo, p.PackageType });
    }
}
