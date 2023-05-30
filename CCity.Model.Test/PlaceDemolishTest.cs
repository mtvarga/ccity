using CCity.Model;

namespace CCity.Model.Test
{
    [TestClass]
    public class PlaceDemolishTest
    {
        private MainModel _model = new MainModel(true,true);

        //Place successful (can place all placeable)
        [TestMethod]
        public void PlaceSuccessfulTest()
        {
            _model.Place(1, 1, new IndustrialZone());
            Assert.IsTrue(_model.Fields[1, 1].Placeable is IndustrialZone);
            _model.Place(1, 2, new CommercialZone());
            Assert.IsTrue(_model.Fields[1, 2].Placeable is CommercialZone);
            _model.Place(1, 3, new ResidentialZone());
            Assert.IsTrue(_model.Fields[1, 3].Placeable is ResidentialZone);
            _model.Place(1, 4, new PoliceDepartment());
            Assert.IsTrue(_model.Fields[1, 4].Placeable is PoliceDepartment);
            _model.Place(2, 1, new FireDepartment());
            Assert.IsTrue(_model.Fields[2, 1].Placeable is FireDepartment);
            _model.Place(2, 2, new Pole());
            Assert.IsTrue(_model.Fields[2, 2].Placeable is Pole);
            _model.Place(2, 3, new Road());
            Assert.IsTrue(_model.Fields[2, 3].Placeable is Road);
            _model.Place(4, 2, new Stadium());
            Assert.IsTrue(_model.Fields[4, 2].Placeable is Stadium);
            _model.Place(7, 5, new PowerPlant());
            Assert.IsTrue(_model.Fields[7, 5].Placeable is PowerPlant);
        }

        //Place Multifield Placeable
        [TestMethod]
        public void PlaceMultifieldtest()
        {
            _model.Place(1, 1, new Stadium());
            Assert.IsTrue(_model.Fields[1, 0].Placeable is Stadium);
            Assert.IsTrue(_model.Fields[2, 1].Placeable is Stadium);
            Assert.IsTrue(_model.Fields[2, 0].Placeable is Stadium);

            _model.Place(3, 1, new PowerPlant());
            Assert.IsTrue(_model.Fields[3, 0].Placeable is PowerPlant);
            Assert.IsTrue(_model.Fields[4, 1].Placeable is PowerPlant);
            Assert.IsTrue(_model.Fields[4, 0].Placeable is PowerPlant);
        }

        //Place unsuccessfully
        [TestMethod]
        public void PlaceUnSucsessulTest()
        {
            _model.Place(_model.Width - 1, _model.Height - 1, new Stadium());
            Assert.AreEqual(GameErrorType.PlaceAlreadyUsedField, _model.LastErrorType);
            _model.Place(-1, 1, new Forest());
            Assert.AreEqual(GameErrorType.PlaceOutOfFieldBoundries, _model.LastErrorType);
            _model.Place(1, 1, new Forest());
            Assert.IsTrue(_model.Fields[1, 1].Placeable is Forest);
            _model.Place(1, 1, new PoliceDepartment());
            Assert.IsFalse(_model.Fields[1, 1].Placeable is PoliceDepartment);
            Assert.AreEqual(GameErrorType.PlaceAlreadyUsedField, _model.LastErrorType);
        }

        //Demolish succesful (can demolish all placeable)

        [TestMethod]
        public void DemolishSuccessfulTest()
        {
            _model.Place(1, 1, new IndustrialZone());
            _model.Place(1, 2, new CommercialZone());
            _model.Place(1, 3, new ResidentialZone());
            _model.Place(1, 4, new PoliceDepartment());
            _model.Place(2, 1, new FireDepartment());
            _model.Place(2, 2, new Pole());
            _model.Place(2, 3, new Road());
            _model.Place(4, 2, new Stadium());
            _model.Place(7, 5, new PowerPlant());

            _model.Demolish(1, 1);
            Assert.IsFalse(_model.Fields[1, 1].HasPlaceable);
            _model.Demolish(1, 2);
            Assert.IsFalse(_model.Fields[1, 2].HasPlaceable);
            _model.Demolish(1, 3);
            Assert.IsFalse(_model.Fields[1, 3].HasPlaceable);
            _model.Demolish(1, 4);
            Assert.IsFalse(_model.Fields[1, 4].HasPlaceable);
            _model.Demolish(2, 1);
            Assert.IsFalse(_model.Fields[1, 1].HasPlaceable);
            _model.Demolish(2, 2);
            Assert.IsFalse(_model.Fields[1, 1].HasPlaceable);
            _model.Demolish(4, 2);
            Assert.IsFalse(_model.Fields[1, 1].HasPlaceable);
            _model.Demolish(7, 5);
            Assert.IsFalse(_model.Fields[1, 1].HasPlaceable);
        }

        //Demolish Multifield Placeable (click on Filler)
        [TestMethod]
        public void DemolishMultifieldTest()
        {
            _model.Place(1, 1, new Stadium());
            _model.Demolish(1, 0);
            Assert.IsFalse(_model.Fields[1, 1].HasPlaceable);

            _model.Place(1, 1, new Stadium());
            _model.Demolish(2, 0);
            Assert.IsFalse(_model.Fields[1, 1].HasPlaceable);

            _model.Place(1, 1, new Stadium());
            _model.Demolish(2, 1);
            Assert.IsFalse(_model.Fields[1, 1].HasPlaceable);


            _model.Place(1, 1, new PowerPlant());
            _model.Demolish(1, 0);
            Assert.IsFalse(_model.Fields[1, 1].HasPlaceable);

            _model.Place(1, 1, new PowerPlant());
            _model.Demolish(1, 0);
            Assert.IsFalse(_model.Fields[1, 1].HasPlaceable);

            _model.Place(1, 1, new PowerPlant());
            _model.Demolish(2, 0);
            Assert.IsFalse(_model.Fields[1, 1].HasPlaceable);

            _model.Place(1, 1, new PowerPlant());
            _model.Demolish(2, 1);
            Assert.IsFalse(_model.Fields[1, 1].HasPlaceable);
        }

        //Demolish unsuccessful
        [TestMethod]
        public void DemolishUnsuccesfulTest()
        {
            _model.Demolish(-1, 1);
            Assert.AreEqual(GameErrorType.DemolishOutOfFieldBoundries, _model.LastErrorType);
            _model.Demolish(1, 1);
            Assert.AreEqual(_model.LastErrorType, GameErrorType.DemolishEmptyField);
            _model.Demolish(_model.Width / 2, _model.Height-1);
            Assert.AreEqual(GameErrorType.DemolishMainRoad, _model.LastErrorType);


            _model.Place(_model.Width / 2, _model.Height - 2, new Road());
            _model.Place(_model.Width / 2-1, _model.Height - 2, new PoliceDepartment());
            _model.Demolish(_model.Width / 2, _model.Height - 2);
            Assert.IsTrue(_model.Fields[_model.Width / 2, _model.Height - 2].Placeable is Road);
            Assert.AreEqual(GameErrorType.DemolishFieldPublicity, _model.LastErrorType);

        }

        //Demolish filed with citizen
        [TestMethod]
        public void DemolishFilledZone()
        {
            ResidentialZone residentialZone = new ResidentialZone();
            Citizen citizen = new Citizen(residentialZone);
            residentialZone.AddCitizen(citizen);
            _model.Place(1,1,residentialZone);
            _model.Demolish(1, 1);
            Assert.AreEqual(GameErrorType.DemolishFieldHasCitizen, _model.LastErrorType);

            IndustrialZone industrialZone = new IndustrialZone();
            industrialZone.AddCitizen(citizen);
            _model.Place(2, 1, industrialZone);
            _model.Demolish(2, 1);
            Assert.AreEqual(GameErrorType.DemolishFieldHasCitizen, _model.LastErrorType);

            CommercialZone commercialZone = new CommercialZone();
            commercialZone.AddCitizen(citizen);
            _model.Place(3, 1, commercialZone);
            _model.Demolish(3, 1);
            Assert.AreEqual(GameErrorType.DemolishFieldHasCitizen, _model.LastErrorType);
        }

    }
}