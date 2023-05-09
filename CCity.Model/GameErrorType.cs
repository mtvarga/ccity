using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CCity.Model
{
    public enum GameErrorType
    {
        PlaceOutOfFieldBoundries,
        PlaceAlreadyUsedField,
        DemolishOutOfFieldBoundries,
        DemolishEmptyField,
        DemolishMainRoad,
        DemolishFieldHasCitizen,
        DemolishFieldPublicity,
        DeployFireTruckNoFire,
        DeployFireTruckOutOfFieldBounds,
        DeployFireTruckBadBuilding,
        DeployFireTruckNoneAvaiable,
        Unhandled
    }
}
