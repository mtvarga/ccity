namespace CCity.Model;

public class PlaceableTransaction : ITransaction
{
    #region Properties
    public Placeable Placeable { get; set; } = null!;
    public bool Add { get; set; }
    public uint Amount { get; set; }
    public PlaceableTransactionType TransactionType { get; set; }
    #endregion
}