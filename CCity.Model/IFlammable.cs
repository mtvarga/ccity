using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CCity.Model
{
    public interface IFlammable
    {
        #region Constants

        internal const ushort FlammableMaxHealth = 400;
        
        #endregion
        
        #region Properties

        // For convenience, we store the potential of a flammable catching fire as an integer between 0 and 100.
        public byte Potential { get; }

        public bool Burning { get; internal set; }
        
        // For convenience, since a building takes about 400 ticks to burn down completely, we store the health of 
        // a flammable as an integer between 0 and 400.
        public ushort Health { get; internal set; }

        #endregion
    }
}
