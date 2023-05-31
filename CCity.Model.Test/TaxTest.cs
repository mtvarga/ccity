namespace CCity.Model.Test
{
    [TestClass]
    public class TaxTest
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
            _model.Place(23,29,new CommercialZone());
            _model.Place(23,28,new CommercialZone());
            _model.Place(23,27,new IndustrialZone());
            _model.Place(23,26,new IndustrialZone());
        }
     
        //Collect Tax
        [TestMethod]
        public void CollectTaxTest()
        {
            _model.ChangeSpeed(Speed.Fast);
            int nextYear = _model.Date.Year + 1;
            while (_model.Date.Year!=nextYear)
            {
                _model.TimerTick();
            }
            
            Assert.IsInstanceOfType(_model.Logbook.ElementAt(4),typeof(TaxTransaction));
            TaxTransaction industrialTax = (TaxTransaction) _model.Logbook.ElementAt(4);
            Assert.AreEqual(TaxType.Industrial,industrialTax.TaxType);
            
            Assert.IsInstanceOfType(_model.Logbook.ElementAt(5),typeof(TaxTransaction));
            TaxTransaction commercialTax = (TaxTransaction) _model.Logbook.ElementAt(5);
            Assert.AreEqual(TaxType.Commercial,commercialTax.TaxType);
            
            Assert.IsInstanceOfType(_model.Logbook.ElementAt(6),typeof(TaxTransaction));
            TaxTransaction residentialTax = (TaxTransaction) _model.Logbook.ElementAt(6);
            Assert.AreEqual(TaxType.Residental,residentialTax.TaxType);
        }
        
        //Change Tax
        [TestMethod]
        public void ChangeTaxTest()
        {
            double residentialTaxBeforeChange = Math.Round(_model.Taxes.ResidentialTax,2);
            double commercialTaxBeforeChange = Math.Round(_model.Taxes.CommercialTax,2);
            double industrialTaxBeforeChange = Math.Round(_model.Taxes.IndustrialTax,2);
            _model.ChangeTax(TaxType.Residental, 0.1);
            _model.ChangeTax(TaxType.Commercial, 0.1);
            _model.ChangeTax(TaxType.Industrial, 0.1);
            Assert.AreEqual( Math.Round(residentialTaxBeforeChange+0.1,2),_model.Taxes.ResidentialTax);
            Assert.AreEqual(Math.Round(commercialTaxBeforeChange+0.1,2),_model.Taxes.CommercialTax);
            Assert.AreEqual(Math.Round(industrialTaxBeforeChange+0.1,2),_model.Taxes.IndustrialTax);
            Assert.IsTrue(Math.Abs(residentialTaxBeforeChange - _model.Taxes.ResidentialTax) > 0);
            Assert.IsTrue(Math.Abs(commercialTaxBeforeChange - _model.Taxes.CommercialTax) > 0);
            Assert.IsTrue(Math.Abs(industrialTaxBeforeChange - _model.Taxes.IndustrialTax) > 0 );
        }
    }
}