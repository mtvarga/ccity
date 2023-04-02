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
        public ResidentialZone Home { get; private set; }
        public WorkplaceZone WorkPlace { get; private set; }
        public double Satisfaction { get; }
        public int LastCalculatedSatisfaction { get; internal set; }
        
        #endregion

        #region Constructors

        public Citizen(ResidentialZone home, WorkplaceZone workplace)
        {
            Home = residentalZone;
            Home.AddCitizen(this);
            WorkPlace = workplaceZone;
            WorkPlace.AddCitizen(this);
        }

        #endregion

        #region Public methods
        
        public void SwapWorkplace(WorkplaceZone workplaceZone)
        {
            this.WorkPlace.DropCitizen(this);
            this.WorkPlace = workplaceZone;
            this.WorkPlace.AddCitizen(this);
        }

        public void MoveOut()
        {
            Home.DropCitizen(this);
            WorkPlace.DropCitizen(this);
        }

        public int CalculateSatisfaction()
        {
            // TODO: Calculate satisfaction
            return 0;
        }
        
        #endregion

        #region Private methods
        

        #endregion

    }
}
