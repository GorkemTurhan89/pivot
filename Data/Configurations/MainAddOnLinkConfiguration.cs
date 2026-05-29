using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Pivot.Models.Entities;

namespace Pivot.Data.Configurations;

public class MainAddOnLinkConfiguration : IEntityTypeConfiguration<MainAddOnLink>
{
    public void Configure(EntityTypeBuilder<MainAddOnLink> builder)
    {
        builder.ToTable("MainAddOnLinks");

        builder.HasKey(l => l.Id);

        builder.HasOne(l => l.MainPlan)
            .WithMany(p => p.AddOnLinks)
            .HasForeignKey(l => l.MainPlanId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(l => l.AddOnPlan)
            .WithMany(p => p.MainLinks)
            .HasForeignKey(l => l.AddOnPlanId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(l => new { l.MainPlanId, l.AddOnPlanId }).IsUnique();
    }
}
