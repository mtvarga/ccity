namespace CCity.Model;

public interface ITransaction
{
    #region Properties

    public bool Add { get; }
    public uint Amount { get; }

    #endregion
}