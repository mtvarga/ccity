using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace CCity.Model.Test
{
    [TestClass]
    public class ElectricitySpreadTest
    {
        private MainModel _model = new MainModel(true, true);

        [TestInitialize]
        public void Initialize()
        {
            LevelBuilder.For(_model)
                .Drag<Road>((20, 28), (22, 28))
                .Drag<Road>((22, 27), (22, 16))
                .Drag<FireDepartment>((16, 24), (18, 24))
                .Drag<FireDepartment>((20, 21), (20, 17))
                .Drag<Stadium>((23, 22), (23, 18))
                .Place<Stadium>(23, 26)
                .Place(20, 24, new PowerPlant(true))
                .Place(17, 28, new PowerPlant(true));
        }

        [TestMethod]
        public void PowerPlantSwitchOnTest()
        {
            Assert.AreEqual(1000, _model.Fields[20, 24].Placeable!.MaxSpreadValue[SpreadType.Electricity]());
            Assert.AreEqual(0, _model.Fields[17, 28].Placeable!.MaxSpreadValue[SpreadType.Electricity]());
            _model.Place(19, 28, new Road());
            Assert.AreEqual(1000, _model.Fields[17, 28].Placeable!.MaxSpreadValue[SpreadType.Electricity]());
            _model.Place(23, 28, new PowerPlant(true));
            Assert.AreEqual(1000, _model.Fields[23, 28].Placeable!.MaxSpreadValue[SpreadType.Electricity]());
        }

        [TestMethod]
        public void PowerPlantSpreadTest1()
        {
            _model.Place(20, 25, new FireDepartment());
            Assert.IsTrue(_model.Fields[20, 25].Placeable!.IsElectrified);
        }

        [TestMethod]
        public void PowerPlantSpreadTest2()
        {
            _model.Place(19, 24, new FireDepartment());
            Assert.IsTrue(_model.Fields[19, 24].Placeable!.IsElectrified);
            Assert.IsTrue(_model.Fields[18, 24].Placeable!.IsElectrified);

            _model.Place(20, 22, new FireDepartment());
            Assert.IsTrue(_model.Fields[20, 22].Placeable!.IsElectrified);
            Assert.IsTrue(_model.Fields[20, 21].Placeable!.IsElectrified);

            _model.Place(23, 24, new Stadium());
            Assert.IsTrue(_model.Fields[23, 24].Placeable!.IsElectrified);
            Assert.IsTrue(_model.Fields[23, 22].Placeable!.IsElectrified);
            Assert.IsTrue(_model.Fields[23, 26].Placeable!.IsElectrified);
        }

        [TestMethod]
        public void PowerPlantSpreadTest3()
        {
            _model.Place(17, 26, new FireDepartment());
            Assert.IsFalse(_model.Fields[17, 26].Placeable!.IsElectrified);
            _model.Place(17, 25, new FireDepartment());
            _model.Place(19, 24, new FireDepartment());
            Assert.IsTrue(_model.Fields[17, 26].Placeable!.IsElectrified);
        }

        [TestMethod]
        public void ElectrifiedPlaceableDemolishTest1()
        {
            _model.Place(19, 24, new FireDepartment());
            _model.Place(18, 25, new FireDepartment());
            _model.Demolish(18, 24);
            Assert.IsFalse(_model.Fields[18, 25].Placeable!.IsElectrified);
        }

        [TestMethod]
        public void ElectrifiedPlaceableDemolishTest2()
        {
            _model.Place(23, 24, new Stadium());
            _model.Demolish(23, 24);
            Assert.IsFalse(_model.Fields[23, 22].Placeable!.IsElectrified);
            Assert.IsFalse(_model.Fields[23, 26].Placeable!.IsElectrified);
        }

        [TestMethod]
        public void PowerPlantCapacityFullTest()
        {
            _model.Place(23, 24, new Stadium());
            Assert.AreEqual(1000, _model.Fields[20, 24].Placeable!.CurrentSpreadValue[SpreadType.Electricity]);
            _model.Place(23, 16, new PoliceDepartment());
            Assert.IsFalse(_model.Fields[23, 16].Placeable!.IsElectrified);
        }

        [TestMethod]
        public void PowerPlantOptimizationTest1()
        {
            _model.Place(23, 24, new Stadium());
            _model.Place(23, 16, new PoliceDepartment());
            _model.Place(25, 26, new PoliceDepartment());
            Assert.IsTrue(_model.Fields[23, 26].Placeable!.IsElectrified);
            Assert.IsFalse(_model.Fields[23, 16].Placeable!.IsElectrified);
            Assert.IsFalse(_model.Fields[23, 18].Placeable!.IsElectrified);
            Assert.IsTrue(_model.Fields[23, 18].Placeable!.IsPartlyElectrified);
        }

        [TestMethod]
        public void PowerPlantOptimizationTest2()
        {
            LevelBuilder.For(_model)
                .Place<FireDepartment>((20, 22), (20, 16), (21, 16), (19, 16), (18, 16))
                .Place<Stadium>(23, 24);
            
            Assert.IsFalse(_model.Fields[18, 16].Placeable!.IsElectrified);
            Assert.AreEqual(1000, _model.Fields[20, 24].Placeable!.CurrentSpreadValue[SpreadType.Electricity]);
            _model.Place(23, 16, new PowerPlant());
            Assert.IsTrue(_model.Fields[18, 16].Placeable!.IsElectrified);
            Assert.AreNotEqual(1000, _model.Fields[20, 24].Placeable!.CurrentSpreadValue[SpreadType.Electricity]);
        }

        [TestMethod]
        public void PowerPlantDemolishTest1()
        {
            LevelBuilder.For(_model)
                .Place<FireDepartment>((20, 22), (20, 16), (21, 16), (19, 16), (18, 16))
                .Place<Stadium>((23, 24))
                .Place(23, 16, new PowerPlant(true));

            Placeable previousSource = _model.Fields[23, 24].Placeable!.GetsSpreadFrom[SpreadType.Electricity].root!;
            _model.Demolish(20, 24);
            Assert.IsTrue(_model.Fields[23, 24].Placeable!.IsElectrified);
            Assert.AreNotSame(previousSource, _model.Fields[23, 24].Placeable!.GetsSpreadFrom[SpreadType.Electricity].root!);
        }

        [TestMethod]
        public void PowerPlantDemolishTest2()
        {
            LevelBuilder.For(_model)
                .Place<FireDepartment>((20, 22), (20, 16), (21, 16), (19, 16), (18, 16))
                .Place<Stadium>((23, 24))
                .Place(23, 16, new PowerPlant(true))
                .Demolish((20, 24), (23, 16));

            for(int x = 16; x <= 24; x++)
                for(int y = 16; y <= 28; y++)
                    if(_model.Fields[x, y].HasPlaceable)
                        Assert.IsFalse(_model.Fields[x, y].Placeable!.IsElectrified);
        }

        //Previous test repeated, mixed up the order of demolished PowerPlants
        [TestMethod]
        public void PowerPlantDemolishTest3()
        {
            LevelBuilder.For(_model)
                .Place<FireDepartment>((20, 22), (20, 16), (21, 16), (19, 16), (18, 16))
                .Place<Stadium>((23, 24))
                .Place(23, 16, new PowerPlant(true))
                .Demolish((20, 24), (23, 16));

            for (int x = 16; x <= 24; x++)
                for (int y = 16; y <= 28; y++)
                    if (_model.Fields[x, y].HasPlaceable)
                        Assert.IsFalse(_model.Fields[x, y].Placeable!.IsElectrified);
        }

        [TestMethod]
        public void PowerPlantCapacityWhenCitizenMovesIn()
        {
            LevelBuilder.For(_model)
                .Place<Road>(19, 28)
                .Place<ResidentialZone>((19, 27), (20, 27))
                .Place<CommercialZone>((21, 27), (21, 26));

            int spreadBeforeMovingIn = _model.Fields[17, 28].Placeable!.CurrentSpreadValue[SpreadType.Electricity];

            DateTime dateTwoYearsLater = _model.Date.AddYears(2);
            while(_model.Population == 0 && !(dateTwoYearsLater.Year == _model.Date.Year && dateTwoYearsLater.Month == _model.Date.Month))
            {
                _model.TimerTick();
            }

            Assert.AreEqual(_model.Population * 2, _model.Fields[17, 28].Placeable!.CurrentSpreadValue[SpreadType.Electricity] - spreadBeforeMovingIn);
        }
    }
}
