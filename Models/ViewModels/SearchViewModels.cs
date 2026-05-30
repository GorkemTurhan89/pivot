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
    public decimal Price { get; set; }
    public string Currency { get; set; } = "GBP";
    public bool IsMandatory { get; set; }
}

public class CartViewModel
{
    public CartMainLine Main { get; set; } = new();
    public decimal? RegistrationFee { get; set; }
    public List<CartAddOnLine> AddOns { get; set; } = new();
    public decimal Total { get; set; }
    public string Currency { get; set; } = "GBP";
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
}
