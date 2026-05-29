namespace Pivot.Models.Entities;

public class School
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;

    public int CityId { get; set; }
    public City City { get; set; } = null!;

    public ICollection<CourseProgram> Programs { get; set; } = new List<CourseProgram>();
    public ICollection<PaymentPlan> PaymentPlans { get; set; } = new List<PaymentPlan>();
}
