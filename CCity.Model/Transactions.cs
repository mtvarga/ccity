namespace CCity.Model;

public static class Transactions
{
    
    #region Constants 
    private const int ResTaxNorm = 150;
    private const int ComTaxNorm = 500;
    private const int IndTaxNorm = 750;
    #endregion
    
    /// <summary>
    /// Returns a transaction that represents a tax collection from a residential zone
    /// </summary>
    /// <param name="taxType"> The type of tax to be collected</param>
    /// <param name="tax"> The tax rate</param>
    /// <returns> A transaction that represents a tax collection from a residential zone</returns>s
    public static TaxTransaction ResidentialTaxCollection(TaxType taxType,double tax)
    {
        return new TaxTransaction
        {
            Amount = unchecked((uint)(Math.Round(ResTaxNorm * tax))),
            TaxType = taxType,
        };
    }
    
    /// <summary>
    ///  Returns a transaction that represents a tax collection from a workplace
    /// </summary>
    /// <param name="taxType"> The type of tax to be collected</param>
    /// <param name="tax"> The tax rate</param>
    /// <param name="citizenCount"> The number of citizens working in the workplace</param>
    /// <returns> A transaction that represents a tax collection from a workplace</returns>
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
            },
        };
    }

    /// <summary>
    /// Returns a transaction that represents a payment for placing a placeable
    /// </summary>
    /// <param name="p"> The placeable to be paid for</param>
    /// <returns> A transaction that represents a payment for placing a placeable</returns>
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

    /// <summary>
    /// Returns a transaction that represents a payment for maintenance of a placeable
    /// </summary>
    /// <param name="p"> The placeable to be paid for</param>
    /// <returns> A transaction that represents a payment for maintenance of a placeable</returns>
    public static PlaceableTransaction Maintenance(Placeable p)
    {
        return new PlaceableTransaction
        {
            Add= false,
            Amount = (uint)p.MaintenanceCost,
            Placeable = p,
            TransactionType = PlaceableTransactionType.Maintenance,
        };
    }

   /// <summary>
   /// Returns a transaction that represents a payment for demolishing a placeable
   /// </summary>
   /// <param name="p"> The placeable to be paid for</param>
   /// <returns> A transaction that represents a payment for demolishing a placeable</returns>
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

   /// <summary>
   /// Returns a transaction that represents a payment for upgrading a placeable
   /// </summary>
   /// <param name="p"> The placeable to be paid for</param>
   /// <param name="cost"> The cost of the upgrade</param>
   /// <returns> A transaction that represents a payment for upgrading a placeable</returns>
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