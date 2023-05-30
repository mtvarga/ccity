using CCity.Model;

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
            _model.Place(20,25,new Stadium());
            _model.Place(19, 27, new Pole());
            _model.Place(23,25,new FireDepartment());
            _model.Place(23,24,new PoliceDepartment());
            _model.Place(24,29,new Forest());
            
            _model.Place(23,29,new CommercialZone());
            _model.Place(23,28,new CommercialZone());
            _model.Place(23,27,new IndustrialZone());
            _model.Place(23,26,new IndustrialZone());
        }
     
        //Collect Tax
        [TestMethod]
        public void CollectTaxTest()
        {
            var currentBudget = _model.Budget;
            _model.ChangeSpeed(Speed.Fast);
            int nextYear = _model.Date.Year + 1;
            while (_model.Date.Year!=nextYear)
            {
                _model.TimerTick();
            }
            
            Assert.IsInstanceOfType(_model.Logbook.ElementAt(7),typeof(TaxTransaction));
            TaxTransaction industrialTax = (TaxTransaction) _model.Logbook.ElementAt(7);
            Assert.AreEqual(TaxType.Industrial,industrialTax.TaxType);
            
            Assert.IsInstanceOfType(_model.Logbook.ElementAt(8),typeof(TaxTransaction));
            TaxTransaction commercialTax = (TaxTransaction) _model.Logbook.ElementAt(8);
            Assert.AreEqual(TaxType.Commercial,commercialTax.TaxType);
            
            Assert.IsInstanceOfType(_model.Logbook.ElementAt(9),typeof(TaxTransaction));
            TaxTransaction residentialTax = (TaxTransaction) _model.Logbook.ElementAt(9);
            Assert.AreEqual(TaxType.Residental,residentialTax.TaxType);
            
            Assert.IsTrue(_model.Budget > currentBudget);
        }
        
        //Change Tax
        [TestMethod]
        public void ChangeTaxTest()
        {
            double residentaltaxBeforeChange = Math.Round(_model.Taxes.ResidentialTax,2);
            double commercialtaxBeforeChange = Math.Round(_model.Taxes.CommercialTax,2);
            double industrialtaxBeforeChange = Math.Round(_model.Taxes.IndustrialTax,2);
            _model.ChangeTax(TaxType.Residental, 0.1);
            _model.ChangeTax(TaxType.Commercial, 0.1);
            _model.ChangeTax(TaxType.Industrial, 0.1);
            Assert.AreEqual( Math.Round(residentaltaxBeforeChange+0.1,2),_model.Taxes.ResidentialTax);
            Assert.AreEqual(Math.Round(commercialtaxBeforeChange+0.1,2),_model.Taxes.CommercialTax);
            Assert.AreEqual(Math.Round(industrialtaxBeforeChange+0.1,2),_model.Taxes.IndustrialTax);
            Assert.IsTrue(residentaltaxBeforeChange != _model.Taxes.ResidentialTax);
            Assert.IsTrue(commercialtaxBeforeChange != _model.Taxes.CommercialTax);
            Assert.IsTrue(industrialtaxBeforeChange != _model.Taxes.IndustrialTax);
        }
    }
}