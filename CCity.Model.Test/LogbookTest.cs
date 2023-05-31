namespace CCity.Model.Test
{
    [TestClass]
    public class LogbookTest
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
            _model.Place(21,28,new ResidentialZone());
            _model.Place(20,27,new PowerPlant());
            _model.Place(20,25,new Stadium());
            _model.Place(19, 27, new Pole());
            _model.Place(23,25,new FireDepartment());
            _model.Place(23,24,new PoliceDepartment());
            _model.Place(24,29,new Forest());
            
            _model.Place(23,29,new CommercialZone());
            _model.Place(23,28,new CommercialZone());
            _model.Place(23,27,new IndustrialZone());
            _model.Place(23,26,new IndustrialZone());
            
            
            _model.ChangeSpeed(Speed.Fast);
            int nextYear = _model.Date.Year + 1;
            while (_model.Date.Year!=nextYear)
            {
                _model.TimerTick();
            }
        }

        [TestMethod]
        //Collect Tax
        public void CollectTaxTest()
        {
            Assert.IsInstanceOfType(_model.Logbook.ElementAt(7),typeof(TaxTransaction));
            TaxTransaction industrialTax = (TaxTransaction) _model.Logbook.ElementAt(7);
            Assert.AreEqual(TaxType.Industrial,industrialTax.TaxType);
            
            Assert.IsInstanceOfType(_model.Logbook.ElementAt(8),typeof(TaxTransaction));
            TaxTransaction commercialTax = (TaxTransaction) _model.Logbook.ElementAt(8);
            Assert.AreEqual(TaxType.Commercial,commercialTax.TaxType);
            
            Assert.IsInstanceOfType(_model.Logbook.ElementAt(9),typeof(TaxTransaction));
            TaxTransaction residentialTax = (TaxTransaction) _model.Logbook.ElementAt(9);
            Assert.AreEqual(TaxType.Residental,residentialTax.TaxType);
        }

        [TestMethod]
        //Collect Maintenance
        public void CollectMaintenanceTest()
        {
            Assert.IsInstanceOfType(_model.Logbook.ElementAt(0),typeof(PlaceableTransaction));
            PlaceableTransaction pole = (PlaceableTransaction) _model.Logbook.ElementAt(0);
            Assert.IsInstanceOfType(pole.Placeable,typeof(Pole));
            Assert.AreEqual(PlaceableTransactionType.Maintenance,pole.TransactionType);
            
            Assert.IsInstanceOfType(_model.Logbook.ElementAt(1),typeof(PlaceableTransaction));
            PlaceableTransaction forest = (PlaceableTransaction) _model.Logbook.ElementAt(1);
            Assert.IsInstanceOfType(forest.Placeable,typeof(Forest));
            Assert.AreEqual(PlaceableTransactionType.Maintenance,forest.TransactionType);
            
            Assert.IsInstanceOfType(_model.Logbook.ElementAt(2),typeof(PlaceableTransaction));
            PlaceableTransaction road = (PlaceableTransaction) _model.Logbook.ElementAt(2);
            Assert.IsInstanceOfType(road.Placeable,typeof(Road));
            Assert.AreEqual(PlaceableTransactionType.Maintenance,road.TransactionType);
            
            Assert.IsInstanceOfType(_model.Logbook.ElementAt(3),typeof(PlaceableTransaction));
            PlaceableTransaction fireDepartment = (PlaceableTransaction) _model.Logbook.ElementAt(3);
            Assert.IsInstanceOfType(fireDepartment.Placeable,typeof(FireDepartment));
            
            Assert.IsInstanceOfType(_model.Logbook.ElementAt(4),typeof(PlaceableTransaction));
            PlaceableTransaction policeDepartment = (PlaceableTransaction) _model.Logbook.ElementAt(4);
            Assert.IsInstanceOfType(policeDepartment.Placeable,typeof(PoliceDepartment));
            
            Assert.IsInstanceOfType(_model.Logbook.ElementAt(5),typeof(PlaceableTransaction));
            PlaceableTransaction powerPlant = (PlaceableTransaction) _model.Logbook.ElementAt(5);
            Assert.IsInstanceOfType(powerPlant.Placeable,typeof(PowerPlant));
            
            Assert.IsInstanceOfType(_model.Logbook.ElementAt(6),typeof(PlaceableTransaction));
            PlaceableTransaction stadium = (PlaceableTransaction) _model.Logbook.ElementAt(6);
            Assert.IsInstanceOfType(stadium.Placeable,typeof(Stadium));
        }
        
        //Add Tax to Logbook
        [TestMethod]
        public void AddTaxToLogbookTest()
        {
            GlobalManager globalManager = new GlobalManager();
            List<ITransaction> transactionList = new List<ITransaction>();
            List<TaxTransaction> taxTransactions = new List<TaxTransaction>();
            taxTransactions.Add( Transactions.WorkplaceTaxCollection(TaxType.Industrial, 1, 1));
            Assert.AreEqual(TaxType.Industrial,taxTransactions[0].TaxType);
            Assert.AreEqual(Convert.ToUInt32(750),taxTransactions[0].Amount);
            Assert.AreEqual(true,taxTransactions[0].Add);
            
            taxTransactions.Add(Transactions.WorkplaceTaxCollection(TaxType.Commercial, 1, 1));
            Assert.AreEqual(TaxType.Commercial,taxTransactions[1].TaxType);
            Assert.AreEqual(Convert.ToUInt32(500),taxTransactions[1].Amount);
            Assert.AreEqual(true,taxTransactions[1].Add);
            
            taxTransactions.Add(Transactions.ResidentialTaxCollection(TaxType.Residental, 1));
            Assert.AreEqual(TaxType.Residental,taxTransactions[2].TaxType);
            Assert.AreEqual(Convert.ToUInt32(150),taxTransactions[2].Amount);
            Assert.AreEqual(true,taxTransactions[2].Add);
            
            transactionList.Add(globalManager.CommitTransaction(taxTransactions[0]));
            transactionList.Add(globalManager.CommitTransaction(taxTransactions[1]));
            transactionList.Add(globalManager.CommitTransaction(taxTransactions[2]));
            globalManager.AddTaxToLogbook(transactionList);
            Assert.AreEqual(3,globalManager.Logbook.Count);

        }
        
        //Add Placeable to Logbook
        [TestMethod]
        public void AddPlaceableToLogbookTest()
        {
            PlaceableTransaction placeableTransaction;
            _model.Place(3,3,new Road());
            var actualPlaceable = _model.Fields[3, 3].ActualPlaceable;
            if (actualPlaceable != null)
                placeableTransaction=Transactions.Placement(actualPlaceable);
            else throw new Exception("No Placeable found");
                Assert.AreEqual(PlaceableTransactionType.Placement,placeableTransaction.TransactionType);
            Assert.AreEqual(Convert.ToUInt32(actualPlaceable.PlacementCost),placeableTransaction.Amount);
            Assert.AreEqual(false,placeableTransaction.Add);
            Assert.IsInstanceOfType(placeableTransaction.Placeable,typeof(Road));
            Assert.IsInstanceOfType(_model.Logbook.ElementAt(0),typeof(PlaceableTransaction));
        }
        
        //Add Takeback to Logbook
        [TestMethod]
        public void AddTakebackToLogbookTest()
        {
            var placeable = _model.Fields[23, 25].ActualPlaceable;
            _model.Demolish(23, 25);
            Assert.IsInstanceOfType(_model.Logbook.ElementAt(0),typeof(PlaceableTransaction));
            PlaceableTransaction takeback = (PlaceableTransaction) _model.Logbook.ElementAt(0);
            Assert.IsInstanceOfType(takeback.Placeable,typeof(FireDepartment));
            Assert.AreEqual(PlaceableTransactionType.Takeback,takeback.TransactionType);
            Assert.AreEqual(Convert.ToUInt32(placeable!.PlacementCost/2),takeback.Amount);
            Assert.AreEqual(true,takeback.Add);
        }
        
        //Add upgrade to Logbook
        [TestMethod]
        public void AddUpgradeToLogbookTest()
        {
            IUpgradeable place = (IUpgradeable) _model.Fields[21, 28].ActualPlaceable!;
            var upgradeCost = place.NextUpgradeCost;
            _model.Upgrade(21 ,28);
            Assert.IsInstanceOfType(_model.Logbook.ElementAt(0),typeof(PlaceableTransaction));
            PlaceableTransaction upgrade = (PlaceableTransaction) _model.Logbook.ElementAt(0);
            Assert.IsInstanceOfType(upgrade.Placeable,typeof(ResidentialZone));
            Assert.AreEqual(PlaceableTransactionType.Upgrade,upgrade.TransactionType);
            Assert.AreEqual(Convert.ToUInt32(upgradeCost),upgrade.Amount);
            Assert.AreEqual(false,upgrade.Add);
        }
    }
}