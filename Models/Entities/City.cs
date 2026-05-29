namespace Pivot.Models.Entities;

public class City
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;

    public int CountryId { get; set; }
    public Country Country { get; set; } = null!;

    public ICollection<School> Schools { get; set; } = new List<School>();
}
