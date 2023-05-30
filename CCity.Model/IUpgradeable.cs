namespace CCity.Model
{
    public interface IUpgradeable
    {
        
        #region Properties

        public  Level Level { get; set; }
        public  int NextUpgradeCost { get; }
        public  bool CanUpgrade { get;}

        #endregion

        #region Public methods

        public void Upgrade()
        {
            if (CanUpgrade)
                Level++;

        }

        #endregion
    }
}