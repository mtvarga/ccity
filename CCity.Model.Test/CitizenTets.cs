using CCity.Model;

namespace CCity.Model.Test
{
    [TestClass]
    public class CitizenTest
    {
        private MainModel _model = new MainModel(true, true);
        [TestInitialize]
        public void Initialize()
        {
            _model.Place(22,28,new Road());
            _model.Place(22,27,new Road());
            _model.Place(22,26,new Road()); 
            _model.Place(22,25,new Road());
            _model.Place(22,24,new Road());
            
            _model.Place(21,29,new ResidentialZone());
            _model.Place(23,29,new CommercialZone());
            _model.Place(23,27,new IndustrialZone());
        }
        
        //Change workplace
        [TestMethod]
        public void ChangeWorkplaceTest()
        {
            Citizen citizen = new Citizen((ResidentialZone) _model.Fields[21,29].Placeable, (WorkplaceZone) _model.Fields[23,27].Placeable);
            var previousWorkplace = citizen.Workplace;
            citizen.ChangeWorkplace((WorkplaceZone) _model.Fields[23,29].Placeable);
            Assert.AreNotEqual(previousWorkplace,citizen.Workplace);
        }

        [TestMethod]
        public void MoveOutTest()
        {
            Citizen citizen = new Citizen((ResidentialZone) _model.Fields[21,29].Placeable, (WorkplaceZone) _model.Fields[23,27].Placeable);
            var workplace = citizen.Workplace;
            var home = citizen.Home;
            var previousCitizenCountAtWorkplace = workplace.Count;
            var previousCitizenCountAtHome = home.Count;
            citizen.MoveOut();
            Assert.AreEqual(previousCitizenCountAtWorkplace-1,workplace.Count);
            Assert.AreEqual(previousCitizenCountAtHome-1,home.Count);
        }
     
    }
}