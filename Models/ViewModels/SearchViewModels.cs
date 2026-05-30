using Pivot.Models.Entities;

namespace Pivot.Models.ViewModels;

public class SearchIndexViewModel
{
    public List<LookupItem> Countries { get; set; } = new();
}

public class LookupItem
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
}

// BÜYÜK GEÇİŞ (2026-05-30): bant fiyat modeli ile bu VM tamamen yeniden şekillendi.
// LengthWeeks/ValidFrom/ValidTo/TotalPrice -> MinWeek/MaxWeek/PriceType + List/Promo birim ve toplam.
public class PaymentPlanListItemViewModel
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int MinWeek { get; set; }
    public int MaxWeek { get; set; }
    public string PriceType { get; set; } = "Weekly"; // "Weekly" | "FixedTotal"
    public decimal ListUnit { get; set; }            // Weekly: haftalık liste ücreti / FixedTotal: liste toplamı
    public decimal? PromoUnit { get; set; }          // varsa promosyonlu birim
    public int? SelectedWeeks { get; set; }
    public decimal? ListTotal { get; set; }          // SelectedWeeks doluyken
    public decimal? PromoTotal { get; set; }
    public string Currency { get; set; } = string.Empty;
    public DateOnly? EndDate { get; set; }
}

public class AddOnListItemViewModel
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Category { get; set; }
    public string PriceType { get; set; } = "Weekly"; // "Weekly" | "FixedTotal"
    public decimal Price { get; set; }                 // Weekly: haftalık ücret / FixedTotal: sabit toplam
    public string Currency { get; set; } = string.Empty;
    public bool IsMandatory { get; set; }
}

// ExtraServices (Vize / Uçak Bileti) sayfası için katalog kaydı.
public class ExtraServiceListItemViewModel
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;   // "Vize" / "UçakBileti"
    public string? VisaType { get; set; }
    public decimal? DefaultPrice { get; set; }              // parse edilen alt sınır
    public string DefaultPriceText { get; set; } = string.Empty;  // Excel'deki ham referans
    public string Currency { get; set; } = string.Empty;
}

// Bir accommodation seçildiğinde dinamik olarak getirilen supplement satırı.
public class SupplementListItemViewModel
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;       // "Summer Supplement"
    public string AppliesTo { get; set; } = string.Empty;  // "Homestay"
    public decimal WeeklyFee { get; set; }
    public string Currency { get; set; } = string.Empty;
    public bool IsConditional { get; set; }                // Required == "Conditional"
    public string? ConditionNotes { get; set; }            // Conditional ise popup'ta gösterilecek
    public DateOnly? StartDate { get; set; }
    public DateOnly? EndDate { get; set; }
}

public class CartViewModel
{
    // cartGuid verildiyse dolu gelir; sepet ekranının üstündeki müşteri kartı için.
    public CartMemberInfo? Member { get; set; }
    public DateOnly StartDate { get; set; }       // kullanıcının seçtiği "kayıt tarihi"
    public CartMainLine Main { get; set; } = new();
    public decimal? RegistrationFee { get; set; }
    public List<CartAddOnLine> AddOns { get; set; } = new();
    // ExtraServices (Vize / UçakBileti) + manuel kalemler. UI'da addon'lardan ayrı blokta.
    public List<CartAddOnLine> Extras { get; set; } = new();
    public decimal Total { get; set; }
    public string Currency { get; set; } = "GBP";
}

// ExtraServices submit'inden gelen JSON kalemler (manuel veya ExtraService).
public class CartExtraInput
{
    public int? Id { get; set; }                       // PaymentPlanId; manual'da null
    public string Name { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;  // "Vize"/"UçakBileti"/"Manuel"
    public decimal Amount { get; set; }
    public string Currency { get; set; } = string.Empty;
}

public class CartMemberInfo
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? Email { get; set; }
    public DateOnly Birthday { get; set; }
}

public class CartMainLine
{
    public string Name { get; set; } = string.Empty;
    public int Weeks { get; set; }
    public decimal WeeklyRate { get; set; }
    public decimal LineTotal { get; set; }       // promo varsa promo, yoksa liste
    public decimal? ListTotal { get; set; }      // promo varsa indirimsiz tutar gösterilir
    public decimal? Discount { get; set; }       // ListTotal - LineTotal (pozitifse)
}

public class CartAddOnLine
{
    public string Name { get; set; } = string.Empty;
    public string? Category { get; set; }
    public decimal Amount { get; set; }
    // Bu addon seçilince ek olarak alınan kerelik bedel (örn. konaklama yerleştirme ücreti).
    // Tutulduğu yer: PaymentPlan.RegistrationFee (yan tabloda değil). UI'da addon altında satır olarak gösterilir.
    public decimal? RegistrationFee { get; set; }
    public bool IsMandatory { get; set; }
    // Weekly addon'lar için kullanıcının seçtiği hafta (per-addon override). FixedTotal'larda 0.
    public int Weeks { get; set; }
}
