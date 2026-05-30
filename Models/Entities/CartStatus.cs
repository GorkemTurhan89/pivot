namespace Pivot.Models.Entities;

// CRM funnel statüsü. Lead aşamasından offer'a ve ileride contractSigned, paid... eklenebilir.
public enum CartStatus
{
    LeadCreated = 1,
    OfferCreated = 2
}
