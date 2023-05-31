using CCity.Model;

namespace CCity.Model.Test
{
    [TestClass]
    public class IncreasePopulationTest
    {
        private MainModel _model = new MainModel(true, true);
        [TestInitialize]
        public void Initialize()
        {
            LevelBuilder.For(_model)
                .Drag<Road>((22, 28), (22, 1))
                .Drag<Pole>((21, 26), (21, 1))
                .Place(20, 28, new PowerPlant());
        }
        //Increase population successfully
        [TestMethod]
        public void IncreasePopulationSuccessfullyTest()
        {
            _model.Place(23,28,new ResidentialZone());
            _model.Place(23,27,new ResidentialZone());
            _model.Place(23,26,new ResidentialZone());
            _model.Place(23,25,new CommercialZone());
            _model.Place(23,24,new CommercialZone());
            _model.Place(23,23,new CommercialZone());
            _model.Place(23,22,new IndustrialZone());
            _model.Place(23,21,new IndustrialZone());
            _model.Place(23,20,new IndustrialZone());

            _model.ChangeSpeed(Speed.Fast);
            int nextYear = _model.Date.Year + 5;
            while (_model.Date.Year!=nextYear)
            {
                _model.TimerTick();
            }
            Assert.AreNotEqual(0,_model.Population);
        }
        
        //No workplace
        [TestMethod]
        public void NoWorkplaceTest()
        {
            _model.Place(23,28,new ResidentialZone());
            _model.ChangeSpeed(Speed.Fast);
            int nextMonth = _model.Date.Month + 1<=12?_model.Date.Month + 1:1;
            
            while (_model.Date.Month!=nextMonth)
            {
                _model.TimerTick();
            }
            Assert.AreEqual(0,_model.Population);
        }
        
        //Increase population until reach minimum,then check the workplace distance
        [TestMethod]
        public void NotIncreaseIfWorkplaceIsTooFar()
        {
            _model.Place(23,28,new ResidentialZone());
            _model.Place(23,27,new ResidentialZone());

            _model.Place(23,1,new IndustrialZone());
            _model.Place(23,2,new IndustrialZone());
            _model.ChangeSpeed(Speed.Fast);
            int nextYear = _model.Date.Year + 5;
            while (_model.Date.Year!=nextYear)
            {
                _model.TimerTick();
            }
            Assert.AreNotEqual(0,_model.Population);
            Assert.AreEqual(16,_model.Population);
            
            //if workplace is close enough, population will increase
            _model.Place(23,26,new IndustrialZone());
            int nextMonth = _model.Date.Month + 1<=12?_model.Date.Month + 1:1;
            while (_model.Date.Month!=nextMonth)
            {
                _model.TimerTick();
            }
            Assert.AreNotEqual(16,_model.Population);
        }
        
        //Workplace balance
        [TestMethod]
        public void CheckWorkPlaceBalanceWhenPopulationIncrease()
        {
            _model.Place(23,28,new ResidentialZone());
            _model.Place(23, 26, new IndustrialZone());
            _model.Place(23,25,new CommercialZone());
            WorkplaceZone industrialZone = (WorkplaceZone) _model.Fields[23,26].Placeable;
            WorkplaceZone commercialZone = (WorkplaceZone) _model.Fields[23,25].Placeable;
            
            _model.ChangeSpeed(Speed.Fast);
            int nextYear = _model.Date.Year + 5;
            while (_model.Date.Year!=nextYear)
            {
                _model.TimerTick();
            }
            Assert.AreEqual(5,industrialZone.Count);
            Assert.AreEqual(5,commercialZone.Count);
        }
    
        //Check desire to move in
        [TestMethod]
        public void CheckDesireToMoveInEffect()
        {
            _model.Place(23,28,new ResidentialZone());
            _model.Place(23,15,new CommercialZone());
            WorkplaceZone commercialZone = (WorkplaceZone) _model.Fields[23,15].Placeable;
            ResidentialZone residentialZone = (ResidentialZone) _model.Fields[23,28].Placeable;
            var startDesireToMoveIn = residentialZone.DesireToMoveIn;
            _model.ChangeSpeed(Speed.Fast);
            int nextYear = _model.Date.Year+3;
            while (_model.Date.Year!=nextYear)
            {
                _model.TimerTick();
            }
            //Desire to move in after move in
            var desireToMoveIn = residentialZone.DesireToMoveIn;
            Assert.AreNotEqual(0,_model.Population);
            Assert.IsTrue(desireToMoveIn>startDesireToMoveIn);
            
            //Desire to move in after place forest
            for (int i = 0; i < residentialZone.Count; i++)
            {
                residentialZone.Citizens[i].MoveOut();
            }
            _model.Place(23,27,new Forest());
            _model.Place(24,28,new Forest());
            nextYear = _model.Date.Year+1;
            while (_model.Date.Year!=nextYear)
            {
                _model.TimerTick();
            }
            var desireToMoveInAfterPlaceForest = residentialZone.DesireToMoveIn;
            Assert.IsTrue(desireToMoveInAfterPlaceForest > desireToMoveIn);
        }
        
        //Not move in when no electricity
        [TestMethod]
        public void NotMoveInWhenNoElectricity()
        {
         _model.Place(23,28,new Road());
         _model.Place(24,28,new Road());
         _model.Place(25,28,new Road());
         _model.Place(26,28,new Road());
         _model.Place(24,27,new ResidentialZone());
         _model.Place(26,27,new CommercialZone());
         ResidentialZone residentialZone = (ResidentialZone) _model.Fields[24,27].Placeable;
         CommercialZone commercialZone = (CommercialZone) _model.Fields[26,27].Placeable;
         var nextYear = _model.Date.Year+1;
         while (_model.Date.Year!=nextYear)
         {
             _model.TimerTick();
         }
         Assert.IsFalse(residentialZone.IsElectrified);
         Assert.IsFalse(commercialZone.IsElectrified);
         Assert.AreEqual(0,_model.Population);
         
         //Only ResidentialZone has electricity
         _model.Place(23,27,new Pole());
         Assert.IsTrue(residentialZone.IsElectrified);
         Assert.IsFalse(commercialZone.IsElectrified);
         
         nextYear = _model.Date.Year+1;
         while (_model.Date.Year!=nextYear)
         {
             _model.TimerTick();
         }
         Assert.AreEqual(0,_model.Population);
         
         //Only CommercialZone has electricity
         _model.Demolish(24,27);
         _model.Demolish(26,27);
         _model.Place(26,27,new ResidentialZone());
         _model.Place(24,27,new CommercialZone());
         residentialZone = (ResidentialZone) _model.Fields[26,27].Placeable;
         commercialZone = (CommercialZone) _model.Fields[24,27].Placeable;
         Assert.IsFalse(residentialZone.IsElectrified);
         Assert.IsTrue(commercialZone.IsElectrified);
         
         nextYear = _model.Date.Year+1;
         while (_model.Date.Year!=nextYear)
         {
             _model.TimerTick();
         }
         Assert.AreEqual(0,_model.Population);
         
         //Both have electricity
         _model.Place(25,27,new Pole());
         Assert.IsTrue(residentialZone.IsElectrified);
         Assert.IsTrue(commercialZone.IsElectrified);
         
         nextYear = _model.Date.Year+1;
         while (_model.Date.Year!=nextYear)
         {
             _model.TimerTick();
         }
         Assert.AreNotEqual(0,_model.Population);
        }
    }
}