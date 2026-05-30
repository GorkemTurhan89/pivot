namespace Pivot.Models.Entities;

// CartDetail'in seçilen paketlerinin snapshot'ı. Her paket (main + addon) ayrı satır.
// PaymentPlanId orijinal referans; geri kalanlar SNAPSHOT — Excel re-import'ta orijinal
// silinse/değişse de eski teklif kayıtları bozulmaz.
public class ChoosenPlanDetail
{
    public int Id { get; set; }

    public int CartDetailId { get; set; }
    public CartDetail CartDetail { get; set; } = null!;

    public int PaymentPlanId { get; set; }   // referans; silinmiş olabilir

    public string Name { get; set; } = string.Empty;
    public string? Category { get; set; }
    public PriceType PriceType { get; set; }
    public int MinWeek { get; set; }
    public int MaxWeek { get; set; }

    public decimal? WeeklyListFee { get; set; }
    public decimal? WeeklyPromoFee { get; set; }
    public decimal? TotalListFee { get; set; }
    public decimal? TotalPromoFee { get; set; }

    public int SelectedWeeks { get; set; }            // 0 = FixedTotal
    public decimal LineAmount { get; set; }           // hesaplanan tutar (RegistrationFee hariç)
    public decimal? RegistrationFee { get; set; }     // yerleştirme/kayıt bedeli (kerelik)
    public string Currency { get; set; } = string.Empty;

    public bool IsDiscounted { get; set; }            // promosyon uygulandı mı
    public bool IsMandatory { get; set; }             // zorunlu addon muydu
    public bool IsMain { get; set; }                  // main paket mi (true) / addon (false)
}
