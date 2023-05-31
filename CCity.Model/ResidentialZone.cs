namespace CCity.Model
{
    public class ResidentialZone : Zone
    {
        #region Constants

        private const float ResidentialZonePotential = 0.01f;
        
        #endregion

        #region Properties

        public override int PlacementCost => 500;
        public override int MaintenanceCost => 0;

        public override float Potential => Owner?.FireDepartmentEffect > 0.5 || Empty ? 0 : ResidentialZonePotential;

        #endregion
    }
}
