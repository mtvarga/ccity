using CCity.Model;

namespace CCity.Model.Test
{
    [TestClass]
    public class DecreasePopulationTest
    {
        private MainModel _model = new MainModel(true, true);
        [TestInitialize]
        public void Initialize()
        {
            LevelBuilder.For(_model)
                .Drag<Road>((22, 28), (22, 1))
                .Drag<Pole>((21, 26), (21, 1));

            _model.Place(20,28,new PowerPlant());
        }
      
        [TestMethod]
        public void DecreasePopulationSuccessfullyTest()
        {
            _model.Place(23,28,new ResidentialZone());
            _model.Place(23,15,new CommercialZone());
            _model.Place(23,14,new IndustrialZone());
            ResidentialZone residentialZone = (ResidentialZone) _model.Fields[23,28].Placeable;
            CommercialZone commercialZone = (CommercialZone) _model.Fields[23,15].Placeable;
            IndustrialZone industrialZone = (IndustrialZone) _model.Fields[23,14].Placeable;
            _model.ChangeSpeed(Speed.Fast);
            int nextYear = _model.Date.Year + 1;
            while (_model.Date.Year!=nextYear)
            {
                _model.TimerTick();
            }
            Assert.AreNotEqual(0,_model.Population);
            var population = _model.Population;
            //Decrease satisfaction
            //Increase taxes to maximum
            _model.ChangeTax(TaxType.Residental,0.23);
            _model.ChangeTax(TaxType.Industrial,0.2);
            _model.ChangeTax(TaxType.Commercial,0.1);
            //Build a lot of Industrial zones
            _model.Place(23,29,new IndustrialZone());
            _model.Place(24,28,new IndustrialZone());
            _model.Place(24,29,new IndustrialZone());
            _model.Place(24,27,new IndustrialZone());
            _model.Place(24,26,new IndustrialZone());
            var nextMonth = _model.Date.Month+2<=12?_model.Date.Month+2:1;
            while (_model.Date.Month!=nextMonth)
            {
                _model.TimerTick();
            }
            Assert.IsTrue(population>_model.Population);
        }
        
        //Move out citizens from wrecked buildings
        [TestMethod]
        public void MoveOutCitizenFromWreckedBuildings()
        {
            _model.Place(23,28,new ResidentialZone());
            _model.Place(23,27,new CommercialZone());
            ResidentialZone residentialZone = (ResidentialZone) _model.Fields[23,28].Placeable;
            _model.ChangeSpeed(Speed.Fast);
            int nextYear = _model.Date.Year + 1;
            int previousPopulation = _model.Population;
            while (_model.Population<10)
            {
                _model.TimerTick();
                if (_model.Population<previousPopulation)
                {
                    break;
                }
                previousPopulation = _model.Population;
            }
            _model.IgniteBuilding(23,28);
            var population = _model.Population;
            var nextMonth = _model.Date.Month+2<=12?_model.Date.Month+2:1;
            
            while (_model.Date.Month!=nextMonth)
            {
                _model.TimerTick();
               
            }
            Assert.IsTrue(population>_model.Population);
        }
       
    }
}