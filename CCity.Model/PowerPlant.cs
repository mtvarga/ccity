using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace CCity.Model
{
    public class PowerPlant : Placeable, IFlammable, IMultifield
    {  
        #region Constants

        private const byte PowerPlantPotential = 1;
        private const int ElectricityCapacity = 1000;
        
        #endregion

        #region Fields

        List<Filler> _occupies;
        bool _tested;

        #endregion

        #region Properties

        public override int PlacementCost => 100;

        public override int MaintenanceCost => 10;

        byte IFlammable.Potential => Owner?.FireDepartmentEffect > 0 ? (byte)0 : PowerPlantPotential;
        
        bool IFlammable.Burning { get; set; }

        ushort IFlammable.Health { get; set; } = IFlammable.FlammableMaxHealth;

        int IMultifield.Width => 2;

        int IMultifield.Height => 2;

        List<Filler> IMultifield.Occupies { get => _occupies; set => _occupies = value; }

        #endregion

        #region Constructor

        public PowerPlant(bool tested = false)
        {
            _occupies = new();
            _tested = tested;
        }

        public override void MakeRoot(SpreadType spreadType)
        {
            if (spreadType != SpreadType.Electricity) return;
            base.MakeRoot(spreadType);
            MaxSpreadValue[spreadType] = () => _tested ? 1000 : ElectricityCapacity;
        }

        #endregion
    }
}
