﻿using System;
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
        private MainModel _model = new MainModel(false);

        [TestInitialize]
        public void Initialize()
        {
            TestUtilities.DragPlacePlaceables(_model, new Road(), (20, 28), (22, 28));
            TestUtilities.DragPlacePlaceables(_model, new Road(), (22, 27), (22, 16));
            TestUtilities.DragPlacePlaceables(_model, new FireDepartment(), (16, 24), (18, 24));
            TestUtilities.DragPlacePlaceables(_model, new FireDepartment(), (20, 21), (20, 17));
            TestUtilities.DragPlacePlaceables(_model, new Stadium(), (23, 22), (23, 18));

            _model.Place(20, 24, new PowerPlant(true));
            _model.Place(17, 28, new PowerPlant(true));
            _model.Place(23, 26, new Stadium());
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
            _model.Place(23, 24, new Stadium());
            List<(int, int)> fireDepartments = new() { (20, 22), (20, 16), (21,16), (19, 16), (18, 16) };
            TestUtilities.PlaceSinglePlaceables(_model, new FireDepartment(), fireDepartments);
            
            Assert.IsFalse(_model.Fields[18, 16].Placeable!.IsElectrified);
            Assert.AreEqual(1000, _model.Fields[20, 24].Placeable!.CurrentSpreadValue[SpreadType.Electricity]);
            _model.Place(23, 16, new PowerPlant());
            Assert.IsTrue(_model.Fields[18, 16].Placeable!.IsElectrified);
            Assert.AreNotEqual(1000, _model.Fields[20, 24].Placeable!.CurrentSpreadValue[SpreadType.Electricity]);
        }

        [TestMethod]
        public void PowerPlantDemolishTest1()
        {
            _model.Place(23, 24, new Stadium());
            List<(int, int)> fireDepartments = new() { (20, 22), (20, 16), (21, 16), (19, 16), (18, 16) };
            TestUtilities.PlaceSinglePlaceables(_model, new FireDepartment(), fireDepartments);
            _model.Place(23, 16, new PowerPlant());

            Placeable previousSource = _model.Fields[23, 24].Placeable!.GetsSpreadFrom[SpreadType.Electricity].root!;
            _model.Demolish(20, 24);
            Assert.IsTrue(_model.Fields[23, 24].Placeable!.IsElectrified);
            Assert.AreNotSame(previousSource, _model.Fields[23, 24].Placeable!.GetsSpreadFrom[SpreadType.Electricity].root!);
        }

        [TestMethod]
        public void PowerPlantDemolishTest2()
        {
            _model.Place(23, 24, new Stadium());
            List<(int, int)> fireDepartments = new() { (20, 22), (20, 16), (21, 16), (19, 16), (18, 16) };
            TestUtilities.PlaceSinglePlaceables(_model, new FireDepartment(), fireDepartments);
            _model.Place(23, 16, new PowerPlant());

            _model.Demolish(20, 24);
            _model.Demolish(23, 16);
            for(int x = 16; x <= 24; x++)
                for(int y = 16; y <= 28; y++)
                    if(_model.Fields[x, y].HasPlaceable)
                        Assert.IsFalse(_model.Fields[x, y].Placeable!.IsElectrified);
        }

        //Previous test repeated, mixed up the order of demolished PowerPlants
        [TestMethod]
        public void PowerPlantDemolishTest3()
        {
            _model.Place(23, 24, new Stadium());
            List<(int, int)> fireDepartments = new() { (20, 22), (20, 16), (21, 16), (19, 16), (18, 16) };
            TestUtilities.PlaceSinglePlaceables(_model, new FireDepartment(), fireDepartments);
            _model.Place(23, 16, new PowerPlant());

            _model.Demolish(23, 16);
            _model.Demolish(20, 24);
            for (int x = 16; x <= 24; x++)
                for (int y = 16; y <= 28; y++)
                    if (_model.Fields[x, y].HasPlaceable)
                        Assert.IsFalse(_model.Fields[x, y].Placeable!.IsElectrified);
        }

        [TestMethod]
        public void PowerPlantCapacityWhenCitizenMovesIn()
        {
            _model.Place(19, 28, new Road());
            _model.Place(19, 27, new ResidentialZone());
            _model.Place(20, 27, new ResidentialZone());
            _model.Place(21, 27, new CommercialZone());
            _model.Place(21, 26, new CommercialZone());

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