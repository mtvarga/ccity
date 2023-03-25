using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CCity.Model
{
    public class Filler
    {
        public IMultifield Main { get; private set; }

        public Filler(IMultifield multifield)
        {
            Main = multifield;
        }
    }
}
