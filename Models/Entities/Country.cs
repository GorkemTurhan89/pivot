namespace Pivot.Models.Entities;

public class Country
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;

    // Ülkenin para birimi (ISO 4217, ör. GBP). Planlardaki tutarlar buradan beslenir.
    public string Currency { get; set; } = string.Empty;

    public ICollection<City> Cities { get; set; } = new List<City>();
}
