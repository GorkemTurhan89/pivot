namespace Pivot.Models.Entities;

// PaymentPlan (Category="Konaklama") satırlarının varyant detayı.
// 1:1 ilişki: PaymentPlanId hem PK hem FK.
public class AccommodationDetail
{
    public int PaymentPlanId { get; set; }
    public PaymentPlan PaymentPlan { get; set; } = null!;

    public string Type { get; set; } = string.Empty;        // Homestay/Residence/Hostel/Studio/...
    public string RoomType { get; set; } = string.Empty;    // Single/Twin/Single Ensuite/...
    public string Board { get; set; } = string.Empty;       // B&B/HB/No meals
    public string? ResidenceName { get; set; }              // Residence ise spesifik bina

    // Supplement eşleşmesi için denormalize edilmiş alanlar (Excel kaynağı, runtime'da
    // join yapmadan SupplementDetail ile string karşılaştırma yapılır).
    public string School { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public string Campus { get; set; } = string.Empty;
    public string Required { get; set; } = string.Empty;     // Excel RequiredStatus
    public string DisplayOption { get; set; } = string.Empty;
}
