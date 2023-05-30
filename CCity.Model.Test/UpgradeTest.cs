using CCity.Model;

namespace CCity.Model.Test
{
    [TestClass]
    public class UpgradeTest
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
            _model.Place(23, 27, new IndustrialZone());
            
        }
        
        //Upgrade Residential Zone
        [TestMethod]
        public void UpgradeResidentalZoneTest()
        {
            ResidentialZone? residentialZone = (ResidentialZone)_model.Fields[21, 29].Placeable!;
            IUpgradeable upgradeable = residentialZone;

            //Before Upgrade
            Assert.AreEqual(100,upgradeable.NextUpgradeCost);
            Assert.AreEqual(10,residentialZone.Capacity);
            Assert.IsTrue(upgradeable.CanUpgrade==true);
            Assert.AreEqual(Level.Beginner,upgradeable.Level);
            
            //1. Upgrade
            _model.Upgrade(21,29);

            Assert.AreEqual(200,upgradeable.NextUpgradeCost);
            Assert.AreEqual(30,residentialZone.Capacity);
            Assert.IsTrue(upgradeable.CanUpgrade==true);
            Assert.AreEqual(Level.Intermediate,upgradeable.Level);
            
            //2. Upgrade
            _model.Upgrade(21,29);
            Assert.AreEqual(0,upgradeable.NextUpgradeCost);
            Assert.AreEqual(100,residentialZone.Capacity);
            Assert.IsTrue(upgradeable.CanUpgrade==false);
            Assert.AreEqual(Level.Advanced,upgradeable.Level);
        }
        
        //Upgrade Commercial Zone
        [TestMethod]
        public void UpgradeCommercialZoneTest()
        {
            CommercialZone? commercialZone = (CommercialZone)_model.Fields[23, 29].Placeable!;
            IUpgradeable upgradeable = commercialZone;

            //Before Upgrade
            Assert.AreEqual(100,upgradeable.NextUpgradeCost);
            Assert.AreEqual(10,commercialZone.Capacity);
            Assert.IsTrue(upgradeable.CanUpgrade==true);
            Assert.AreEqual(Level.Beginner,upgradeable.Level);
            
            //1. Upgrade
            _model.Upgrade(23,29);

            Assert.AreEqual(200,upgradeable.NextUpgradeCost);
            Assert.AreEqual(30,commercialZone.Capacity);
            Assert.IsTrue(upgradeable.CanUpgrade==true);
            Assert.AreEqual(Level.Intermediate,upgradeable.Level);
            
            //2. Upgrade
            _model.Upgrade(23,29);
            Assert.AreEqual(0,upgradeable.NextUpgradeCost);
            Assert.AreEqual(100,commercialZone.Capacity);
            Assert.IsTrue(upgradeable.CanUpgrade==false);
            Assert.AreEqual(Level.Advanced,upgradeable.Level);
        }
        
        //Upgrade Industrial Zone
        [TestMethod]
        public void UpgradeIndustrialZoneTest()
        {
            IndustrialZone? industrialZone = (IndustrialZone)_model.Fields[23, 27].Placeable!;
            IUpgradeable upgradeable = industrialZone;

            //Before Upgrade
            Assert.AreEqual(100,upgradeable.NextUpgradeCost);
            Assert.AreEqual(10,industrialZone.Capacity);
            Assert.IsTrue(upgradeable.CanUpgrade==true);
            Assert.AreEqual(Level.Beginner,upgradeable.Level);
            
            //1. Upgrade
            _model.Upgrade(23,27);

            Assert.AreEqual(200,upgradeable.NextUpgradeCost);
            Assert.AreEqual(30,industrialZone.Capacity);
            Assert.IsTrue(upgradeable.CanUpgrade==true);
            Assert.AreEqual(Level.Intermediate,upgradeable.Level);
            
            //2. Upgrade
            _model.Upgrade(23,27);
            Assert.AreEqual(0,upgradeable.NextUpgradeCost);
            Assert.AreEqual(100,industrialZone.Capacity);
            Assert.IsTrue(upgradeable.CanUpgrade==false);
            Assert.AreEqual(Level.Advanced,upgradeable.Level);
        }
    }
}