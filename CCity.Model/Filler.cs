using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CCity.Model
{
    public class Filler : Placeable
    {
        public IMultifield Main { get; private set; }

        public override int PlacementCost => 0;

        public override int MaintenanceCost => 0;

        public Filler(IMultifield multifield)
        {
            Main = multifield;
        }
    }
}
