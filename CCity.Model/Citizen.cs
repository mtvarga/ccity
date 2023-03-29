using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CCity.Model
{
    public class Citizen
    {

        #region Fields


        #endregion

        #region Properties

        public string Name { get; }
        public ResidentialZone Home { get; private set;}
        public WorkplaceZone WorkPlace { get; private set;}
        public double Satisfaction { get; }
        
        #endregion

        #region Constructors

        public Citizen(ResidentialZone residentalZone,WorkplaceZone workplaceZone)
        {
            this.Home = residentalZone;
            this.WorkPlace = workplaceZone;
        }

        #endregion

        #region Public methods
        
        public void SwapWorkplace(WorkplaceZone workplaceZone)
        {
            this.WorkPlace = workplaceZone;
        }

        public void MoveOut()
        {
            throw new NotImplementedException();
        }

        #endregion

        #region Private methods



        #endregion

    }
}
