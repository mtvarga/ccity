namespace CCity.Model.Test;

[TestClass]
public class SatisfactionTest
{
    private MainModel Model { get; } = new(true, true);
    
    [TestInitialize]
    public void Initialize()
    {
        LevelBuilder.For(Model)
            // Place roads
            .Drag<Road>((22, 28), (22, 18))
            // Place zones
            .Drag<ResidentialZone>((21, 26), (21, 23))
            .Drag<CommercialZone>((21, 22), (21, 21))
            .Drag<IndustrialZone>((21, 20), (21, 19))
            // Place power plant
            .Place(20, 28, new PowerPlant());
        
        // Pass the time until at least 40 people move in
        Model.ChangeSpeed(Speed.Fast);
        
        while (Model.Population < 40)
            Model.TimerTick();
    }

    [TestMethod]
    public void TestStadium()
    {
        while (Model.Budget < 10000)
            Model.TimerTick();
        
        var oldSatisfaction = Model.Satisfaction;

        LevelBuilder.For(Model)
            .Place<Stadium>(23, 26);
        
        Assert.IsTrue(oldSatisfaction < Model.Satisfaction, "Stadium did not increase total satisfaction.");

        LevelBuilder.For(Model)
            .Demolish(23, 26);
    }

    [TestMethod]
    public void TestPolice()
    {
        var oldSatisfaction = Model.Satisfaction;

        LevelBuilder.For(Model)
            .Place<PoliceDepartment>(23, 26);
        
        Assert.IsTrue(oldSatisfaction < Model.Satisfaction, "Police department did not increase total satisfaction.");

        LevelBuilder.For(Model)
            .Demolish(23, 26);
    }

    [TestMethod]
    public void TestTaxChange()
    {
        var oldSatisfaction = Model.Satisfaction;

        Model.ChangeTax(TaxType.Residental, -0.1);
        
        Assert.IsTrue(oldSatisfaction < Model.Satisfaction, "Decreasing residential tax did not increase total satisfaction.");

        oldSatisfaction = Model.Satisfaction;
        
        Model.ChangeTax(TaxType.Commercial, -0.1);
        
        Assert.IsTrue(oldSatisfaction < Model.Satisfaction, "Decreasing commercial tax did not increase total satisfaction.");
        
        oldSatisfaction = Model.Satisfaction;
        
        Model.ChangeTax(TaxType.Industrial, -0.04);
        
        Assert.IsTrue(oldSatisfaction < Model.Satisfaction, "Decreasing industrial tax did not increase total satisfaction.");

        oldSatisfaction = Model.Satisfaction;
        
        Model.ChangeTax(TaxType.Residental, 0.3);
        
        Assert.IsTrue(oldSatisfaction > Model.Satisfaction, "Increasing residential tax did not decrease total satisfaction.");

        oldSatisfaction = Model.Satisfaction;
        
        Model.ChangeTax(TaxType.Commercial, 0.2);
        
        Assert.IsTrue(oldSatisfaction > Model.Satisfaction, "Increasing commercial tax did not decrease total satisfaction.");
        
        oldSatisfaction = Model.Satisfaction;
        
        Model.ChangeTax(TaxType.Industrial, 0.2);
        
        Assert.IsTrue(oldSatisfaction > Model.Satisfaction, "Increasing industrial tax did not decrease total satisfaction.");
    }


    [TestMethod]
    public void TestIndustrialZoneNearby()
    {
        var higherResidentialZone = Model.Fields[21, 23];
        var lowerResidentialZone = Model.Fields[21, 26];
        
        Assert.IsTrue(lowerResidentialZone.IndustrialEffect < higherResidentialZone.IndustrialEffect, "The satisfaction of a zone that is closer to an industrial zone is not lower than that of one farther from an industrial zone.");
    }

    [TestMethod]
    public void TestNegativeBudget()
    {
        LevelBuilder.For(Model)
            .Place<Stadium>((23, 28), (23, 26), (23, 24));
        
        Model.ChangeTax(TaxType.Residental, -0.26); // 1%
        Model.ChangeTax(TaxType.Commercial, -0.09); // 1%
        Model.ChangeTax(TaxType.Industrial, -0.04); // 1%

        var thresholdDate = Model.Date.AddYears(3);
        var oldSatisfaction = Model.Satisfaction;
        
        while (Model.Date < thresholdDate)
            Model.TimerTick();
        
        Assert.IsTrue(Model.Satisfaction < oldSatisfaction, "Negative budget did not decrease satisfaction.");
    }

    [TestMethod]
    public void TestForest()
    {
        LevelBuilder.For(Model)
            .Place(23, 26, new Forest())
            .Place(23, 25, new Forest())
            .Place(23, 24, new Forest())
            .Place(23, 23, new Forest());

        var thresholdDate = Model.Date.AddYears(11);
        var oldSatisfaction = Model.Satisfaction;
        
        while (Model.Date < thresholdDate)
            Model.TimerTick();
        
        Assert.IsTrue(oldSatisfaction < Model.Satisfaction, "Fully grown forest did not increase total satisfaction.");
    }
}