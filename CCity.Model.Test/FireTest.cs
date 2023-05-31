namespace CCity.Model.Test;

[TestClass]
public class FireTest
{
    private MainModel Model { get; } = new(true, true);

    [TestInitialize]
    public void Initialize()
    {
        // Build test level
        LevelBuilder.For(Model)
            // Place roads
            .Drag<Road>((22, 16), (22, 28))
            .Drag<Road>((19, 16), (21, 16))
            .Drag<Road>((23, 16), (25, 16))
            .Drag<Road>((15, 20), (21, 20))
            .Drag<Road>((23, 20), (29, 20))
            .Drag<Road>((19, 24), (21, 24))
            .Drag<Road>((23, 24), (25, 24))
            // Place residential zones
            .Place<ResidentialZone>(19, 23)
            .Drag<ResidentialZone>((15, 19), (16, 19))
            .Drag<ResidentialZone>((28, 19), (29, 19))
            // Place commercial zones
            .Drag<CommercialZone>((28, 21), (29, 21))
            // Place industrial zones
            .Drag<IndustrialZone>((15, 21), (16, 21))
            // Place power plant
            .Place(20, 19, new PowerPlant())
            // Place police departments
            .Place<PoliceDepartment>((21, 17), (23, 19), (25, 21))
            // Place stadium
            .Place<Stadium>(23, 18)
            // Place poles
            .Drag<Pole>((17, 19), (19, 19))
            .Drag<Pole>((24, 19), (27, 19))
            .Drag<Pole>((23, 21), (23, 22));

        // Set speed to fast
        Model.ChangeSpeed(Speed.Fast);
        
        // Pass time until at least 30 people have moved in
        while (Model.Population < 25)
            Model.TimerTick();
    }

    [TestMethod]
    public void TestRandomFire()
    {
        var randomFireModel = new MainModel(true);
        var fireEmergencyPresent = false;
        
        LevelBuilder.For(randomFireModel)
            .Place<PoliceDepartment>(22, 28);
        
        randomFireModel.FieldsUpdated += delegate(object? _, FieldEventArgs args)
        {
            foreach (var field in args.Fields)
            {
                if (field.Placeable is not IFlammable { Burning: true }) 
                    continue;
                
                fireEmergencyPresent = true;
                break;
            }
        };

        randomFireModel.ChangeSpeed(Speed.Fast);

        // Highly likely that something will catch on fire in 1000 years in game
        var thresholdDate = randomFireModel.Date.AddYears(1000);
        
        while (!fireEmergencyPresent && randomFireModel.Date < thresholdDate)
            randomFireModel.TimerTick();
        
        Assert.IsTrue(fireEmergencyPresent);
    }

    [TestMethod]
    public void TestPotentials()
    {
        // Remove fire departments if they have been placed
        LevelBuilder.For(Model)
            .DemolishIf(f => f.Placeable is FireDepartment, (17, 21), (23, 23));

            // 1. Industrial zones have a higher chance to catch fire than other zones
        var residentialZone = (ResidentialZone)Model.Fields[16, 19].Placeable!;
        var commercialZone = (CommercialZone)Model.Fields[28, 21].Placeable!;
        var industrialZone = (IndustrialZone)Model.Fields[16, 21].Placeable!;
        
        Assert.IsTrue(Math.Round(residentialZone.Potential, 2) <= Math.Round(industrialZone.Potential, 2));
        Assert.IsTrue(Math.Round(commercialZone.Potential, 2) <= Math.Round(industrialZone.Potential, 2));
        
        // 2. Empty zones cannot catch on fire
        var emptyResidentialZone = (ResidentialZone)Model.Fields[19, 23].Placeable!;
        Assert.AreEqual(Math.Round(emptyResidentialZone.Potential, 2), 0);
        
        // 3. Nearby fire department decreases potential
        
        // Place fire departments
        LevelBuilder.For(Model)
            .Place<FireDepartment>((17, 21), (23, 23));
        
        // These flammables are close to a fire department 
        var lowerBunch = new List<IFlammable>
        {
            (IFlammable)Model.Fields[15, 19].Placeable!,
            (IFlammable)Model.Fields[16, 19].Placeable!,
            (IFlammable)Model.Fields[15, 21].Placeable!,
            (IFlammable)Model.Fields[16, 21].Placeable!
        };

        // These flammable are far from any fire departments
        var higherBunch = new List<IFlammable>
        {
            (IFlammable)Model.Fields[28, 19].Placeable!,
            (IFlammable)Model.Fields[29, 19].Placeable!,
            (IFlammable)Model.Fields[28, 21].Placeable!,
            (IFlammable)Model.Fields[29, 21].Placeable!
        };

        foreach (var lower in lowerBunch)
        foreach (var higher in higherBunch)
            Assert.IsTrue(Math.Round(lower.Potential, 2) <= Math.Round(higher.Potential, 2));
    }

    [TestMethod]
    public void TestDeployFireTruck()
    {
        var fireEmergencyPresent = false;

        void FieldsUpdatedHandler(object? sender, FieldEventArgs args)
        {
            foreach (var field in args.Fields)
            {
                if (field.Placeable is not IFlammable { Burning: true }) 
                    continue;
                
                fireEmergencyPresent = true;
                return;
            }

            fireEmergencyPresent = false;
        }

        Model.FieldsUpdated += FieldsUpdatedHandler;
        Model.IgniteBuilding(23, 18); // Stadium
        
        var thresholdDate = Model.Date.AddYears(1000);
        
        Model.ChangeSpeed(Speed.Fast);
        
        while (!fireEmergencyPresent && Model.Date < thresholdDate)
            Model.TimerTick();
        
        Assert.IsTrue(fireEmergencyPresent, "Building was not ignited.");

        // Send firetruck
        Model.DeployFireTruck(23, 18);
        
        while (fireEmergencyPresent && Model.Date < thresholdDate)
            Model.TimerTick();
        
        Assert.IsFalse(fireEmergencyPresent, "Fire truck did not put out target.");

        Model.FieldsUpdated -= FieldsUpdatedHandler;
    }

    [TestMethod]
    public void TestAdvancedDeployFireTruck()
    {
        LevelBuilder.For(Model)
            .DemolishIf(f => f.Placeable is FireDepartment, (23, 23))
            .Demolish(17, 21)
            .Place<FireDepartment>(17, 21);
        
        var fireEmergencyPresent = false;

        void FieldsUpdatedHandler(object? sender, FieldEventArgs args)
        {
            foreach (var field in args.Fields)
            {
                if (field.Placeable is not IFlammable { Burning: true }) 
                    continue;
                
                fireEmergencyPresent = true;
                return;
            }

            fireEmergencyPresent = false;
        }

        Model.FieldsUpdated += FieldsUpdatedHandler;
        Model.IgniteBuilding(23, 18); // Stadium
        Model.IgniteBuilding(25, 21); // Police department

        var thresholdDate = Model.Date.AddYears(1000);
        
        Model.ChangeSpeed(Speed.Fast);
        
        while (!fireEmergencyPresent && Model.Date < thresholdDate)
            Model.TimerTick();
        
        Assert.IsTrue(fireEmergencyPresent, "Buildings were not ignited.");

        // Send firetruck
        Model.DeployFireTruck(23, 18); // Stadium

        var exceptionThrown = false;
        
        try
        {
            Model.DeployFireTruck(25, 21); // Police department
        }
        catch (Exception e)
        {
            exceptionThrown = true;
            
            Assert.IsInstanceOfType(e, typeof(GameErrorException));
            Assert.AreEqual(GameErrorType.DeployFireTruckNoneAvaiable, ((GameErrorException)e).ErrorType);
        }

        Assert.IsFalse(exceptionThrown, "Multiple fire trucks were sent from the same fire department.");
        
        while (Model.FireTruckLocations().Any() && Model.Date < thresholdDate)
            Model.TimerTick();
        
        Assert.IsFalse(Model.FireTruckLocations().Any(), "Fire truck did not return to station.");
        
        Model.DeployFireTruck(25, 21); // Police department

        while (fireEmergencyPresent && Model.Date < thresholdDate)
            Model.TimerTick();
        
        Assert.IsFalse(fireEmergencyPresent, "Fire truck did not put out second target.");
        
        Model.FieldsUpdated -= FieldsUpdatedHandler;
    }

    [TestMethod]
    public void TestSimultaneousDeployFireTruck()
    {
        LevelBuilder.For(Model)
            .Demolish((17, 21), (23, 23))
            .Place<FireDepartment>((17, 21), (23, 23));
        
        var fireEmergencyPresent = false;

        void FieldsUpdatedHandler(object? sender, FieldEventArgs args)
        {
            foreach (var field in args.Fields)
            {
                if (field.Placeable is not IFlammable { Burning: true }) 
                    continue;
                
                fireEmergencyPresent = true;
                return;
            }

            fireEmergencyPresent = false;
        }

        Model.FieldsUpdated += FieldsUpdatedHandler;
        Model.IgniteBuilding(23, 18); // Stadium
        Model.IgniteBuilding(25, 21); // Police department

        var thresholdDate = Model.Date.AddYears(1000);
        
        Model.ChangeSpeed(Speed.Fast);
        
        while (!fireEmergencyPresent && Model.Date < thresholdDate)
            Model.TimerTick();
        
        Assert.IsTrue(fireEmergencyPresent, "Buildings were not ignited.");

        // Send firetruck
        Model.DeployFireTruck(23, 18); // Stadium
        Model.DeployFireTruck(25, 21); // Police department

        Assert.AreEqual(2, Model.FireTruckLocations().Count());

        while (fireEmergencyPresent && Model.Date < thresholdDate)
            Model.TimerTick();

        Assert.IsFalse(fireEmergencyPresent, "Fire trucks did not put out targets.");
        
        Model.FieldsUpdated -= FieldsUpdatedHandler;
    }

    [TestMethod]
    public void TestFireDamage()
    {
        LevelBuilder.For(Model)
            .DemolishIf(f => f.Placeable is FireDepartment, (17, 21), (23, 23));
        
        var fireEmergencyPresent = false;
        var buildingTookDamage = false;
        var fireSpread = false;
        var populationDecreased = false;

        var oldPopulation = Model.Population;

        var burningBuildings = new HashSet<IFlammable>();

        void FieldsUpdatedHandler(object? sender, FieldEventArgs args)
        {
            var found = false;
            
            foreach (var field in args.Fields)
            {
                if (field.Placeable is not IFlammable { Burning: true } flammable)
                    continue;

                found = true;

                if (flammable.Health == IFlammable.FlammableMaxHealth) 
                    continue;
                
                if (burningBuildings.Contains(flammable))
                    buildingTookDamage = true;
                else
                    fireSpread = true;
            }

            fireEmergencyPresent = found;
        }

        void PopulationChangedHandler(object? sender, EventArgs args)
        {
            if (Model.Population < oldPopulation)
                populationDecreased = true;
        }
        
        Model.FieldsUpdated += FieldsUpdatedHandler;
        Model.PopulationChanged += PopulationChangedHandler;

        Model.IgniteBuilding(23, 18); // Stadium
        Model.IgniteBuilding(29, 21); // Commercial zone
        
        burningBuildings.Add((IFlammable)Model.Fields[23, 18].Placeable!);
        burningBuildings.Add((IFlammable)Model.Fields[29, 21].Placeable!);

        var thresholdDate = Model.Date.AddYears(1000);
        
        Model.ChangeSpeed(Speed.Fast);
        
        while (!fireEmergencyPresent && Model.Date < thresholdDate)
            Model.TimerTick();
        
        Assert.IsTrue(fireEmergencyPresent, "Buildings were not ignited.");

        do
            Model.TimerTick();
        while (fireEmergencyPresent && Model.Date < thresholdDate);
        
        // 1. Test if fire damages buildings
        Assert.IsTrue(buildingTookDamage, "Building did not take damage.");
        
        // 2. Test if fire spread
        Assert.IsTrue(fireSpread, "Fire did not spread");
        
        // 3. Test if buildings burned down
        Assert.IsFalse(fireEmergencyPresent, "Buildings did not burn down.");
        
        // 4. Test if citizens of burned down zones moved out 
        Assert.IsTrue(populationDecreased, "Population did not decrease.");
    }
}