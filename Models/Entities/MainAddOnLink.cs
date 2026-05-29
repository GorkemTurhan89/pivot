namespace Pivot.Models.Entities;

// Ana paket (Main) ile ek paket (AddOn) arasındaki çoğa-çok bağlantı.
// Her iki FK de PaymentPlan'a referans verir (kendine referanslı M2M).
public class MainAddOnLink
{
    public int Id { get; set; }

    public int MainPlanId { get; set; }
    public PaymentPlan MainPlan { get; set; } = null!;

    public int AddOnPlanId { get; set; }
    public PaymentPlan AddOnPlan { get; set; } = null!;
}
