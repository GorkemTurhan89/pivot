using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Pivot.Models.Entities;

namespace Pivot.Data.Configurations;

public class CartDetailConfiguration : IEntityTypeConfiguration<CartDetail>
{
    public void Configure(EntityTypeBuilder<CartDetail> builder)
    {
        builder.ToTable("CartDetails");
        builder.HasKey(c => c.Id);

        builder.Property(c => c.CartGuid).IsRequired();
        builder.HasIndex(c => c.CartGuid).IsUnique();

        builder.Property(c => c.PersonalId).HasMaxLength(50);
        builder.Property(c => c.Nationality).IsRequired().HasMaxLength(100);
        builder.Property(c => c.Email).HasMaxLength(200);
        builder.Property(c => c.PhoneNumber).HasMaxLength(30);

        builder.Property(c => c.Status).IsRequired().HasConversion<int>();
        builder.Property(c => c.TotalPaymentPrice).HasPrecision(18, 2);

        builder.HasOne(c => c.Member)
            .WithMany()
            .HasForeignKey(c => c.MemberId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(c => c.MemberId);
        builder.HasIndex(c => c.Status);
    }
}
