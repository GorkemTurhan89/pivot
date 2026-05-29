namespace Pivot.Models.Entities;

public class CourseProgram
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;

    public int SchoolId { get; set; }
    public School School { get; set; } = null!;

    public ICollection<PaymentPlan> PaymentPlans { get; set; } = new List<PaymentPlan>();
}
