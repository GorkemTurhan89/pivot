namespace Pivot.Models.ViewModels;

public class QuoteViewModel
{
    public Guid CartGuid { get; set; }
    public DateTime CreatedAt { get; set; }
    public bool AutoPrint { get; set; }

    // Müşteri
    public string CustomerFullName { get; set; } = string.Empty;
    public string? CustomerEmail { get; set; }
    public string? CustomerPhone { get; set; }
    public string? CustomerNationality { get; set; }
    public DateOnly? CustomerBirthday { get; set; }

    // Satış temsilcisi (login olan kullanıcı)
    public string SalesRepName { get; set; } = string.Empty;
    public string? SalesRepEmail { get; set; }
    public string SalesRepRole { get; set; } = string.Empty;

    // Kurs/okul info
    public string SchoolName { get; set; } = string.Empty;
    public string? CityName { get; set; }
    public string? CountryName { get; set; }
    public string? ProgramName { get; set; }
    public int MainWeeks { get; set; }
    public string Currency { get; set; } = string.Empty;

    // Kalemler
    public List<QuoteLine> CourseLines { get; set; } = new();
    public decimal CourseTotal { get; set; }

    public List<QuoteLine> AccommodationLines { get; set; } = new();
    public decimal AccommodationTotal { get; set; }
    public bool HasAccommodation => AccommodationLines.Count > 0;

    public List<QuoteLine> ExtraLines { get; set; } = new();
    public bool HasExtras => ExtraLines.Count > 0;

    public decimal GrandTotal { get; set; }
}

public class QuoteLine
{
    public string Label { get; set; } = string.Empty;
    public string? Detail { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; } = string.Empty;
}
