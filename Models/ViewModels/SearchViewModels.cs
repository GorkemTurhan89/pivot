namespace Pivot.Models.ViewModels;

public class SearchIndexViewModel
{
    public List<LookupItem> Countries { get; set; } = new();
}

public class LookupItem
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
}

public class PaymentPlanListItemViewModel
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int LengthWeeks { get; set; }
    public DateOnly ValidFrom { get; set; }
    public DateOnly ValidTo { get; set; }
    public decimal TotalPrice { get; set; }
    public string Currency { get; set; } = "GBP";
    public DateOnly EndDate { get; set; }
}

public class AddOnListItemViewModel
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Category { get; set; }
    public decimal Price { get; set; }
    public string Currency { get; set; } = "GBP";
    public bool IsMandatory { get; set; }
}

public class CartViewModel
{
    public CartMainLine Main { get; set; } = new();
    public decimal? RegistrationFee { get; set; }
    public List<CartAddOnLine> AddOns { get; set; } = new();
    public decimal Total { get; set; }
    public string Currency { get; set; } = "GBP";
}

public class CartMainLine
{
    public string Name { get; set; } = string.Empty;
    public int Weeks { get; set; }
    public decimal WeeklyRate { get; set; }
    public decimal LineTotal { get; set; }
}

public class CartAddOnLine
{
    public string Name { get; set; } = string.Empty;
    public string? Category { get; set; }
    public decimal Amount { get; set; }
    public bool IsMandatory { get; set; }
}
