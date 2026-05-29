namespace Pivot.Models.Entities;

public class PaymentPlan
{
    public int Id { get; set; }

    public int SchoolId { get; set; }
    public School School { get; set; } = null!;

    public int? ProgramId { get; set; }
    public CourseProgram? Program { get; set; }

    public string Name { get; set; } = string.Empty;

    public int LengthWeeks { get; set; }

    public DateOnly ValidFrom { get; set; }
    public DateOnly ValidTo { get; set; }

    public PackageType PackageType { get; set; } = PackageType.Main;

    public bool IsAdditional { get; set; }

    // Ek paketler için: "Konaklama", "Transfer" vb. Ana paketlerde null.
    public string? Category { get; set; }

    public decimal? MembershipFee { get; set; }
    public decimal? RegistrationFee { get; set; }
    public decimal? PromotedFee { get; set; }

    // Çift amaçlı: AddOn'da true => bu ek paket zorunlu.
    // Main'de true => zorunlu ek hizmeti var (gate); false ise zorunlu AddOn'lara hiç bakılmaz.
    public bool IsOrHasMandatory { get; set; }

    public bool IsActive { get; set; } = true;

    public ICollection<PaymentPlanItem> Items { get; set; } = new List<PaymentPlanItem>();

    // Bu paket Main ise: ona bağlı ek paket linkleri.
    public ICollection<MainAddOnLink> AddOnLinks { get; set; } = new List<MainAddOnLink>();

    // Bu paket AddOn ise: bağlı olduğu ana paket linkleri.
    public ICollection<MainAddOnLink> MainLinks { get; set; } = new List<MainAddOnLink>();
}
