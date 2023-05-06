using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Security;
using System.Text;
using System.Threading.Tasks;

namespace CCity.Model
{
    public interface IUpgradeable
    {
        #region Properties

        public Level Level { get; set; }
        public int NextUpgradeCost { get; }
        public bool CanUpgrade { get; }

        #endregion

        #region Public methods

        public void Upgrade()
        {
            if (CanUpgrade) Level++;
        }

        #endregion
    }
}
