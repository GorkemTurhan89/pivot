namespace Pivot.Models.Entities;

// Lead/kayıt formundan gelen müşteri bilgisi.
// Email ve PhoneNumber nullable + unique (MySQL: birden çok NULL kabul edilir,
// dolu değerler unique olmak zorunda).
public class MemberDetail
{
    public int Id { get; set; }

    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;

    public string? PersonalId { get; set; }     // TC/pasaport, opsiyonel
    public string Nationality { get; set; } = string.Empty;

    public DateOnly Birthday { get; set; }

    public string? Email { get; set; }
    public string? PhoneNumber { get; set; }    // "+90..." prefix dahil saklanır

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
