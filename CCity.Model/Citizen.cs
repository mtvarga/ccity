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
        public WorkplaceZone Workplace { get; private set; }
        public double Satisfaction { get; }
        public int LastCalculatedSatisfaction { get; internal set; }
        
        #endregion

        #region Constructors

        public Citizen(ResidentialZone home, WorkplaceZone workplace)
        {
            Home = home;
            Home.AddCitizen(this);
            Workplace = workplace;
            Workplace.AddCitizen(this);
        }

        #endregion

        #region Public methods
        
        public void SwapWorkplace(WorkplaceZone workplace)
        {
            Workplace.DropCitizen(this);
            Workplace = workplace;
            Workplace.AddCitizen(this);
        }

        public void MoveOut()
        {
            Home.DropCitizen(this);
            Workplace.DropCitizen(this);
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
