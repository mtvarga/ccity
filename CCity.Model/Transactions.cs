using System.Text;

namespace CCity.Model;

public static class Transactions
{
    private const int ResTaxNorm = 150;
    private const int ComTaxNorm = 500;
    private const int IndTaxNorm = 750;

    public static TaxTransaction ResidentialTaxCollection(TaxType taxType,double tax)
    {
        return new TaxTransaction
        {
            Amount = unchecked((uint)(Math.Round(ResTaxNorm * tax))),
            TaxType = taxType,
        };
    }
    public static TaxTransaction WorkplaceTaxCollection(TaxType taxType,double tax,int citizenCount)
    {
       
        return new TaxTransaction
        {
            TaxType = taxType,
            Amount = taxType switch
            {
                TaxType.Commercial => unchecked((uint)(Math.Round(ComTaxNorm * tax * citizenCount))),
                TaxType.Industrial => unchecked((uint)(Math.Round(IndTaxNorm * tax * citizenCount))),
                _ => 0
            }
        };
    }

    public static PlaceableTransaction Placement(Placeable p)
    {
        return new PlaceableTransaction
        {
            Add= false,
            Amount = (uint)p.PlacementCost,
            Placeable = p,
            TransactionType = PlaceableTransactionType.Placement,
        };
    }

    public static PlaceableTransaction Maintance(Placeable p)
    {
        return new PlaceableTransaction
        {
            Add= false,
            Amount = (uint)p.MaintenanceCost,
            Placeable = p,
            TransactionType = PlaceableTransactionType.Maintenance,
        };
    }

    public static PlaceableTransaction Takeback(Placeable p)
    {
        return new PlaceableTransaction
        {
            Add= true,
            Amount = (uint)(p.PlacementCost/2),
            Placeable = p,
            TransactionType = PlaceableTransactionType.Takeback,
        };
    }

    public static PlaceableTransaction Upgrade(Placeable p,int cost)
    {
        return new PlaceableTransaction
        {
            Add= false,
            Amount = (uint)cost,
            Placeable = p,
            TransactionType = PlaceableTransactionType.Upgrade,
        };
    }
}