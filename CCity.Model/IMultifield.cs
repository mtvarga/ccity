using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CCity.Model
{
    public interface IMultifield
    {

        #region Properties

        public int Width { get; }
        public int Height { get; }
        public List<Filler> Occupies { get; internal set; }

        #endregion

    }
}
