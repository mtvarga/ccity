namespace CCity.Model;

public class TaxTransaction : ITransaction  
{
    #region Properties

    public bool Add { get; set; } = true;
    public TaxType TaxType { get; set; }
    public uint Amount { get; set; }

    #endregion
}