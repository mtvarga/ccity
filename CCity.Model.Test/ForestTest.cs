using CCity.Model;
using System.Diagnostics;

namespace CCity.Model.Test
{
    [TestClass]
    public class ForestTest
    {
        private MainModel _model = new MainModel(true,true);

        //Forest generation
        [TestMethod]
        public void ForestGenerationTest()
        {
            _model = new MainModel(false,true);
            bool containsForest = false;
            foreach (Field field in _model.Fields)
            {
                if (field.Placeable is Forest)
                {
                    containsForest = true;
                    break;
                }
            }
            Assert.IsTrue(containsForest);
        }

        //Forest effect spreaded
        [TestMethod]
        public void ForestEffectSpreadedTest()
        {
            _model.Place(1, 1, new Forest());
            Forest forest = (Forest)_model.Fields[1, 1].Placeable!;
            Assert.IsTrue(forest.IsPublic);
            Assert.IsTrue(_model.Fields[2, 1].ForestEffect > 0);
            Assert.IsTrue(_model.Fields[1, 2].ForestEffect > 0);
            Assert.IsTrue(_model.Fields[1, 3].ForestEffect > 0);
            Assert.IsTrue(_model.Fields[1, 4].ForestEffect > 0);
        }

        //Forest not visible test
        [TestMethod]
        public void ForestNotVisibleTest()
        {
            _model.Place(1, 1, new Forest());
            Assert.IsTrue(_model.Fields[1, 3].ForestEffect > 0);
            _model.Place(1, 2, new PoliceDepartment());
            Assert.AreEqual(0,_model.Fields[1, 3].ForestEffect);
            _model.Demolish(1, 2);
            Assert.IsTrue(_model.Fields[1, 3].ForestEffect > 0);
        }

        //Forest effect revoked
        [TestMethod]
        public void ForestEffectRevokedTest()
        {
            _model.Place(1, 1, new Forest());
            _model.Demolish(1, 1);
            Assert.AreEqual(0,_model.Fields[2, 1].ForestEffect);
            Assert.AreEqual(0,_model.Fields[1, 2].ForestEffect );
            Assert.AreEqual(0,_model.Fields[1, 3].ForestEffect );
            Assert.AreEqual(0,_model.Fields[1, 4].ForestEffect);
        }

        //IndustrialZone effect reduced when Forest placed
        [TestMethod]
        public void IndustrialZoneEffectReductionTest()
        {
            int ROOTX = _model.Width / 2;
            int ROOTY = _model.Height - 1;

            _model.Place(ROOTX - 1, ROOTY, new IndustrialZone());
            _model.Place(ROOTX + 1, ROOTY, new PowerPlant());
            double previusEffect = _model.Fields[ROOTX - 1, ROOTY - 2].IndustrialEffect;
            _model.Place(ROOTX - 1, ROOTY - 1, new Forest());
            Assert.IsTrue(previusEffect > _model.Fields[ROOTX - 1, ROOTY - 2].IndustrialEffect);
            Assert.IsTrue(_model.Fields[ROOTX - 1, ROOTY - 2].IndustrialEffect < _model.Fields[ROOTX - 3, ROOTY].IndustrialEffect);

            _model.Demolish(ROOTX - 1, ROOTY);

            //first place forest then industrialZone
            _model.Place(ROOTX - 1, ROOTY, new IndustrialZone());
            Assert.IsTrue(_model.Fields[ROOTX - 1, ROOTY - 2].IndustrialEffect < _model.Fields[ROOTX - 3, ROOTY].IndustrialEffect);
        }

        //Fores ageing
        [TestMethod]
        public void ForestAgeingTest()
        {
            _model.Place(1, 1, new Forest());
            Forest forest = (Forest)_model.Fields[1, 1].Placeable!;
            Assert.AreEqual(0, forest.Age);
            for (int i = 0; i < 12; i++) forest.Grow();
            Assert.AreEqual(1, forest.Age);

            //age is not over 10
            for (int i = 0; i < 120; i++) forest.Grow();
            Assert.IsFalse(forest.CanGrow);
            Assert.AreEqual(10, forest.Age);
        }

    }
}