namespace Pivot.Models.Entities;

// PaymentPlan (Category="Vize" veya "UçakBileti") satırlarının detayı.
// 1:1 ilişki: PaymentPlanId hem PK hem FK.
// ExtraServices okula bağlı değil — Country + Currency burada saklanır
// (PaymentPlan.SchoolId null olduğundan Country.Currency'ye join yapılamaz).
public class ExtraServiceDetail
{
    public int PaymentPlanId { get; set; }
    public PaymentPlan PaymentPlan { get; set; } = null!;

    public string Country { get; set; } = string.Empty;   // Ülke filtresi (DB Country.Name ile eşleşir)
    public string? VisaType { get; set; }                  // "F-1", "Student Visa"... uçakta null
    public string DefaultPriceText { get; set; } = string.Empty;  // Excel'deki ham metin ("£115-120"); UI'da referans
    public string Currency { get; set; } = string.Empty;
}
