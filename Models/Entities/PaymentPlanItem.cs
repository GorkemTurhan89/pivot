namespace Pivot.Models.Entities;

public class PaymentPlanItem
{
    public int Id { get; set; }

    public int PaymentPlanId { get; set; }
    public PaymentPlan PaymentPlan { get; set; } = null!;

    public string ItemName { get; set; } = string.Empty;
    public decimal ItemPrice { get; set; }
    public string Currency { get; set; } = "GBP";
    public int Quantity { get; set; } = 1;
}
