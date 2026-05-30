namespace Pivot.Models.Entities;

// PaymentPlan (Category="Supplement") satırlarının eşleşme/koşul detayı.
// 1:1 ilişki: PaymentPlanId hem PK hem FK.
// Eşleşme runtime'da AccommodationDetail ile (School+Country+Campus+AppliesTo == Type).
public class SupplementDetail
{
    public int PaymentPlanId { get; set; }
    public PaymentPlan PaymentPlan { get; set; } = null!;

    public string School { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public string Campus { get; set; } = string.Empty;

    // Hangi konaklama tipine uygulanır (örn. "Homestay").
    public string AppliesTo { get; set; } = string.Empty;

    // "Summer Supplement" / "Christmas Supplement" / "U18 Supplement" / "Special Diet Surcharge"
    public string SupplementName { get; set; } = string.Empty;

    // "Conditional" / "Optional"
    public string Required { get; set; } = string.Empty;

    // Conditional ise UI'da popup'ta gösterilecek koşul açıklaması.
    public string? Notes { get; set; }

    // Sezon başlangıç/bitiş (boş olabilir — yıl boyu uygulanan supplement'lar).
    public DateOnly? StartDate { get; set; }
    public DateOnly? EndDate { get; set; }
}
