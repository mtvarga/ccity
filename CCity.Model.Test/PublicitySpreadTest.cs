using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CCity.Model.Test
{
    [TestClass]
    public class PublicitySpreadTest
    {
        private MainModel _model = new MainModel(true);

        [TestInitialize]
        public void Initialize()
        {
            LevelBuilder.For(_model)
                .Drag<Road>((22, 28), (16, 28))
                .Drag<Road>((20, 26), (24, 26))
                .Drag<Road>((20, 24), (24, 24))
                .Drag<Road>((16, 24), (18, 24))
                .Place<Road>((22, 27), (20, 25), (24, 25), (16, 27), (16, 25), (18, 25), (18, 26), (17, 26))
                .Place<PoliceDepartment>(21, 22)
                .Place<Stadium>(25, 23);
        }

        //Testing if root road is public
        [TestMethod]
        public void RootRoadTest()
        {
            Assert.IsTrue(_model.Fields[22, 29].Placeable!.IsPublic);
        }

        //Testing publicity spreading
        [TestMethod]
        public void PublicitySpreadingTest()
        {
            Assert.IsTrue(_model.Fields[22, 28].Placeable!.IsPublic);
            Assert.IsTrue(_model.Fields[16, 28].Placeable!.IsPublic);
            Assert.IsTrue(_model.Fields[22, 26].Placeable!.IsPublic);
            Assert.IsFalse(_model.Fields[17, 26].Placeable!.IsPublic);
            Assert.IsFalse(_model.Fields[16, 25].Placeable!.IsPublic);
            _model.Place(16, 26, new Road());
            Assert.IsTrue(_model.Fields[17, 26].Placeable!.IsPublic);
            Assert.IsTrue(_model.Fields[16, 25].Placeable!.IsPublic);
            Assert.IsTrue(_model.Fields[18, 24].Placeable!.IsPublic);
        }

        [TestMethod]
        public void SingleDemolishTest()
        {
            Assert.IsTrue(_model.Fields[20, 28].Placeable!.IsPublic);
            _model.Demolish(21, 28);
            Assert.IsFalse(_model.Fields[20, 28].Placeable!.IsPublic);
        }

        [TestMethod]
        public void DoubleDemolishTest()
        {
            Assert.IsTrue(_model.Fields[21, 28].Placeable!.IsPublic);
            Assert.IsTrue(_model.Fields[22, 27].Placeable!.IsPublic);
            _model.Demolish(22, 28);
            Assert.IsFalse(_model.Fields[21, 28].Placeable!.IsPublic);
            Assert.IsFalse(_model.Fields[22, 27].Placeable!.IsPublic);
        }

        [TestMethod]
        public void TripleDemolishTest()
        {
            Assert.IsTrue(_model.Fields[21, 26].Placeable!.IsPublic);
            Assert.IsTrue(_model.Fields[23, 26].Placeable!.IsPublic);
            Assert.IsTrue(_model.Fields[22, 27].Placeable!.IsPublic);
            _model.Demolish(22, 26);
            Assert.IsFalse(_model.Fields[21, 26].Placeable!.IsPublic);
            Assert.IsFalse(_model.Fields[23, 26].Placeable!.IsPublic);
            Assert.IsTrue(_model.Fields[22, 27].Placeable!.IsPublic);
        }

        [TestMethod]
        public void QuadrupleDemolishTest()
        {
            _model.Place(15, 26, new Road());
            _model.Place(16, 26, new Road());

            Assert.IsTrue(_model.Fields[15, 26].Placeable!.IsPublic);
            Assert.IsTrue(_model.Fields[17, 26].Placeable!.IsPublic);
            Assert.IsTrue(_model.Fields[16, 25].Placeable!.IsPublic);
            Assert.IsTrue(_model.Fields[16, 27].Placeable!.IsPublic);
            _model.Demolish(16, 26);
            Assert.IsFalse(_model.Fields[15, 26].Placeable!.IsPublic);
            Assert.IsFalse(_model.Fields[17, 26].Placeable!.IsPublic);
            Assert.IsFalse(_model.Fields[16, 25].Placeable!.IsPublic);
            Assert.IsTrue(_model.Fields[16, 27].Placeable!.IsPublic);
        }

        [TestMethod]
        public void SingleCircleDemolishTest1()
        {
            _model.Place(16, 26, new Road());
            Assert.IsTrue(_model.Fields[18, 24].Placeable!.IsPublic);
            _model.Demolish(16, 25);
            Assert.IsTrue(_model.Fields[18, 24].Placeable!.IsPublic);
            _model.Demolish(17, 26);
            Assert.IsFalse(_model.Fields[18, 24].Placeable!.IsPublic);
        }

        [TestMethod]
        public void SingleCircleDemolishTest2()
        {
            _model.Place(16, 26, new Road());
            Assert.IsTrue(_model.Fields[18, 24].Placeable!.IsPublic);
            _model.Demolish(17, 26);
            Assert.IsTrue(_model.Fields[18, 24].Placeable!.IsPublic);
            _model.Demolish(16, 25);
            Assert.IsFalse(_model.Fields[18, 24].Placeable!.IsPublic);
        }

        [TestMethod]
        public void DoubleCircleDemolishTest1()
        {
            _model.Place(22, 25, new Road());
            Assert.IsTrue(_model.Fields[22, 25].Placeable!.IsPublic);
            _model.Demolish(22, 26);
            Assert.IsFalse(_model.Fields[22, 25].Placeable!.IsPublic);
            Assert.IsFalse(_model.Fields[24, 26].Placeable!.IsPublic);
            Assert.IsFalse(_model.Fields[23, 26].Placeable!.IsPublic);
            Assert.IsFalse(_model.Fields[20, 24].Placeable!.IsPublic);
            Assert.IsFalse(_model.Fields[24, 24].Placeable!.IsPublic);
        }

        [TestMethod]
        public void DoubleCircleDemolishTest2()
        {
            _model.Place(22, 25, new Road());
            Assert.IsTrue(_model.Fields[22, 25].Placeable!.IsPublic);
            _model.Demolish(21, 26);
            Assert.IsTrue(_model.Fields[20, 25].Placeable!.IsPublic);
            Assert.IsTrue(_model.Fields[22, 26].Placeable!.IsPublic);
        }

        [TestMethod]
        public void PlaceableGetsPublicity()
        {
            _model.Place(22, 23, new Road());
            Assert.IsFalse(_model.Fields[21, 22].Placeable!.IsPublic);
            _model.Place(22, 22, new Road());
            Assert.IsTrue(_model.Fields[21, 22].Placeable!.IsPublic);
        }

        [TestMethod]
        public void MultifieldPlaceableGetsPublicityTest1()
        {
            Assert.IsFalse(_model.Fields[25, 23].Placeable!.IsPublic);
            Assert.IsFalse(_model.Fields[26, 22].Placeable!.IsPublic);
            _model.Place(24, 23, new Road());
            Assert.IsTrue(_model.Fields[25, 23].Placeable!.IsPublic);
            Assert.IsTrue(_model.Fields[26, 22].Placeable!.IsPublic);
        }

        [TestMethod]
        public void MultifieldPlaceableGetsPublicityTest2()
        {
            Assert.IsFalse(_model.Fields[25, 23].Placeable!.IsPublic);
            Assert.IsFalse(_model.Fields[26, 22].Placeable!.IsPublic);
            _model.Place(25, 25, new Road());
            _model.Place(26, 25, new Road());
            _model.Place(26, 24, new Road());
            Assert.IsTrue(_model.Fields[25, 23].Placeable!.IsPublic);
            Assert.IsTrue(_model.Fields[26, 22].Placeable!.IsPublic);
        }

        //Testing if it is not possible to remove the road the Placeable gets
        //the publicity from indirectly
        [TestMethod]
        public void DemolishRoadToPublicPlaceableTest1()
        {
            _model.Place(22, 23, new Road());
            _model.Place(22, 22, new Road());
            _model.Demolish(22, 23);
            Assert.IsNotNull(_model.Fields[22, 23].Placeable);
            Assert.AreEqual(GameErrorType.DemolishFieldPublicity, _model.LastErrorType);
            _model.Demolish(22, 22);
            Assert.IsNotNull(_model.Fields[22, 22].Placeable);
            Assert.AreEqual(GameErrorType.DemolishFieldPublicity, _model.LastErrorType);
        }

        //Testing if it is possible to remove the road the Placeable gets
        //the publicity from indirectly if there is an other possible source
        [TestMethod]
        public void DemolishRoadToPublicPlaceableTest2()
        {
            _model.Place(22, 23, new Road());
            _model.Place(22, 22, new Road());
            _model.Place(20, 23, new Road());
            _model.Place(20, 22, new Road());
            _model.Demolish(22, 23);
            Assert.IsNull(_model.Fields[22, 23].Placeable);
        }


        //Same logic as DemolishRoadToPublicPlaceableTest1's
        [TestMethod]
        public void DemolishRoadToPublicMultifieldTest1()
        {
            _model.Place(25, 25, new Road());
            _model.Place(26, 25, new Road());
            _model.Place(26, 24, new Road());
            _model.Demolish(25, 25);
            Assert.IsNotNull(_model.Fields[25, 25].Placeable);
            Assert.AreEqual(GameErrorType.DemolishFieldPublicity, _model.LastErrorType);
            _model.Demolish(26, 24);
            Assert.IsNotNull(_model.Fields[26, 24].Placeable);
            Assert.AreEqual(GameErrorType.DemolishFieldPublicity, _model.LastErrorType);
        }

        //Same logic as DemolishRoadToPublicPlaceableTest2's
        [TestMethod]
        public void DemolishRoadToPublicMultifieldTest2()
        {
            _model.Place(25, 25, new Road());
            _model.Place(26, 25, new Road());
            _model.Place(26, 24, new Road());
            _model.Place(24, 23, new Road());
            _model.Demolish(25, 25);
            Assert.IsNull(_model.Fields[25, 25].Placeable);
        }
    }
}
