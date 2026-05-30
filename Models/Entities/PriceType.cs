namespace Pivot.Models.Entities;

// Bant fiyat tipi: Weekly => haftalık ücret × seçilen hafta; FixedTotal => sabit toplam.
public enum PriceType
{
    Weekly = 1,
    FixedTotal = 2
}
