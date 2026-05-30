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
}
