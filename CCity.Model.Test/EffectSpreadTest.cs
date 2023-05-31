using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CCity.Model.Test
{
    [TestClass]
    public class EffectSpreadTest
    {
        private MainModel _model = new MainModel(true, true);

        [TestInitialize]
        public void Initialize()
        {
            LevelBuilder.For(_model)
                .Drag<Road>((22, 28), (22, 16))
                .Drag<Road>((21, 24), (13, 24))
                .Drag<Pole>((23, 28), (23, 18))
                .Drag<Pole>((18, 21), (18, 27))
                .Drag<Pole>((19, 25), (21, 25))
                .Place(23, 17, new PowerPlant());
        }

        [TestMethod]
        public void PlaceableSpreadsEffectIfPublicTest1()
        {
            _model.Place(20, 22, new PoliceDepartment());
            Assert.AreEqual(0.0, _model.Fields[20, 28].PoliceDepartmentEffect);
            _model.Place(21, 22, new Road());
            Assert.AreEqual(0.0, _model.Fields[20, 28].PoliceDepartmentEffect);
            _model.Place(21, 21, new Pole());
            Assert.AreEqual(1, _model.Fields[20, 22].PoliceDepartmentEffect);
        }

        [TestMethod]
        public void PlaceableSpreadsEffectIfPublicTest2()
        {
            _model.Place(21, 23, new PoliceDepartment());
            Assert.AreEqual(1, _model.Fields[21, 23].PoliceDepartmentEffect);
        }

        [TestMethod]
        public void AllExpectedPlaceableSpreadsEffectTest()
        {
            _model.Place(20, 23, new Stadium());
            _model.Place(19, 23, new PoliceDepartment());
            _model.Place(21, 21, new FireDepartment());
            _model.Place(21, 20, new IndustrialZone());
            Field checkedField = _model.Fields[19, 22];
            Assert.IsTrue(checkedField.StadiumEffect > 0);
            Assert.IsTrue(checkedField.PoliceDepartmentEffect > 0);
            Assert.IsTrue(checkedField.FireDepartmentEffect > 0);
            Assert.IsTrue(checkedField.IndustrialEffect > 0);
        }

        [TestMethod]
        public void EffectRevokedWhenNotElectrifiedTest()
        {
            _model.Place(20, 23, new Stadium());
            _model.Place(19, 23, new PoliceDepartment());
            _model.Place(21, 21, new FireDepartment());
            _model.Place(21, 20, new IndustrialZone());
            Field checkedField = _model.Fields[19, 22];
            _model.Demolish(23, 19);
            Assert.AreEqual(0, checkedField.StadiumEffect);
            Assert.AreEqual(0, checkedField.PoliceDepartmentEffect);
            Assert.AreEqual(0, checkedField.FireDepartmentEffect);
            Assert.AreEqual(0, checkedField.IndustrialEffect);
        }

        [TestMethod]
        public void EffectRevokedWhenDemolishedTest()
        {
            _model.Place(20, 23, new Stadium());
            Field checkedField = _model.Fields[19, 22];
            Assert.IsTrue(checkedField.StadiumEffect > 0);
            _model.Demolish(20, 23);
            Assert.AreEqual(0, checkedField.StadiumEffect);
        }

        [TestMethod]
        public void EffectSpreadIntersectionOnPlaceTest()
        {
            _model.Place(21, 23, new PoliceDepartment());
            double effectBeforeSecondPlacement = _model.Fields[21, 20].PoliceDepartmentEffect;
            _model.Place(21, 17, new PoliceDepartment());
            Assert.IsTrue(
                _model.Fields[21, 20].PoliceDepartmentEffect > effectBeforeSecondPlacement ||
                _model.Fields[21, 20].PoliceDepartmentEffect == effectBeforeSecondPlacement && effectBeforeSecondPlacement == 1
            );            
            Assert.AreEqual(Math.Min(1, effectBeforeSecondPlacement * 2), _model.Fields[21, 20].PoliceDepartmentEffect);
        }

        [TestMethod]
        public void EffectSpreadIntersectionOnDemolishTest()
        {
            _model.Place(21, 23, new PoliceDepartment());
            double effectBeforeSecondPlacement = _model.Fields[21, 20].PoliceDepartmentEffect;
            _model.Place(21, 17, new PoliceDepartment());
            Assert.IsTrue(_model.Fields[21, 20].PoliceDepartmentEffect > effectBeforeSecondPlacement);
            _model.Demolish(21, 17);
            Assert.AreEqual(effectBeforeSecondPlacement, _model.Fields[21, 20].PoliceDepartmentEffect);
        }
    }
}
