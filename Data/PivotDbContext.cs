using Microsoft.EntityFrameworkCore;
using Pivot.Models.Entities;

namespace Pivot.Data;

public class PivotDbContext : DbContext
{
    public PivotDbContext(DbContextOptions<PivotDbContext> options) : base(options) { }

    public DbSet<Country> Countries => Set<Country>();
    public DbSet<City> Cities => Set<City>();
    public DbSet<School> Schools => Set<School>();
    public DbSet<CourseProgram> Programs => Set<CourseProgram>();
    public DbSet<PaymentPlan> PaymentPlans => Set<PaymentPlan>();
    public DbSet<PaymentPlanItem> PaymentPlanItems => Set<PaymentPlanItem>();
    public DbSet<MainAddOnLink> MainAddOnLinks => Set<MainAddOnLink>();
    public DbSet<AccommodationDetail> AccommodationDetails => Set<AccommodationDetail>();
    public DbSet<SupplementDetail> SupplementDetails => Set<SupplementDetail>();
    public DbSet<MemberDetail> MemberDetails => Set<MemberDetail>();
    public DbSet<CartDetail> CartDetails => Set<CartDetail>();
    public DbSet<ChoosenPlanDetail> ChoosenPlanDetails => Set<ChoosenPlanDetail>();
    public DbSet<ExtraServiceDetail> ExtraServiceDetails => Set<ExtraServiceDetail>();
    public DbSet<User> Users => Set<User>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(PivotDbContext).Assembly);

        modelBuilder.Entity<User>(b =>
        {
            b.Property(u => u.Username).HasMaxLength(64).IsRequired();
            b.Property(u => u.Email).HasMaxLength(256).IsRequired();
            b.Property(u => u.PasswordHash).HasMaxLength(512).IsRequired();
            b.HasIndex(u => u.Username).IsUnique();
            b.HasIndex(u => u.Email).IsUnique();
        });

        foreach (var entity in modelBuilder.Model.GetEntityTypes())
        {
            entity.SetTableName(entity.GetTableName()!.ToLower());
        }

        base.OnModelCreating(modelBuilder);
    }
}
