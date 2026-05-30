namespace Pivot.Models.Entities;

// CRM cart/teklif kaydı. Lead oluştuğunda hemen düşer (LeadCreated),
// kullanıcı sepete kadar gelirse OfferCreated'a evrilir + ChoosenPlanDetail rows yazılır.
// MemberDetail ↔ CartDetail = 1:N (aynı üyenin birden çok teklif kaydı olabilir).
//
// SNAPSHOT alanları (PersonalId/Nationality/Email/PhoneNumber/RegisterDate):
// üye ileride bilgilerini değiştirse de bu cart'ın o anki haline sabit kalır.
public class CartDetail
{
    public int Id { get; set; }

    public DateTime CreateDate { get; set; } = DateTime.UtcNow;
    public DateTime UpdateDate { get; set; } = DateTime.UtcNow;

    public Guid CartGuid { get; set; } = Guid.NewGuid();

    public int MemberId { get; set; }
    public MemberDetail Member { get; set; } = null!;

    public string? PersonalId { get; set; }
    public DateOnly RegisterDate { get; set; }   // üyenin ilk kayıt tarihi snapshot
    public string Nationality { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? PhoneNumber { get; set; }

    public CartStatus Status { get; set; } = CartStatus.LeadCreated;
    public decimal TotalPaymentPrice { get; set; }

    public ICollection<ChoosenPlanDetail> ChoosenPlans { get; set; } = new List<ChoosenPlanDetail>();
}
