namespace CCity.Model;

public class FireTruck
{
    #region Constants
    
    private const int RescueCounterMax = MainModel.TicksPerSecond * 2; // 2 seconds
    
    #endregion
    
    #region Properties
    
    public Field Location => PathCurrentNode?.Value ?? Station;
    
    internal Field Station { get; }
    
    // There are 4 possible states of a fire truck, based on the combination of the below 2 properties:
    // - Active: false, Moving: false => the fire truck is standing by at its fire department
    // - Active: true, Moving: true => the fire truck is currently on its way to where fire broke out
    // - Active: true, Moving: false => the fire truck is currently extinguishing the building that is on fire
    // - Active: false, Moving: true => the fire truck is currently returning to its fire department

    // Currently attending to a fire emergency
    internal bool Active { get; private set; }
    
    // Currently on the move
    internal bool Moving { get; private set; }

    internal bool Deployed => Active || Moving;

    internal bool DepartedFromStation { get; private set; }
    
    private LinkedListNode<Field>? PathCurrentNode { get; set; }
    
    private LinkedList<Field>? Path { get; set; }

    // For convenience reasons, the return path should be a list whose **FIRST** item is the fire truck's home department,
    // and last item is the location of the fire emergency that the fire truck is assigned to.
    //
    // This is so because if the fire truck is ordered to a fire emergency from its home department, the return path
    // is the same list as the fire truck's path to the fire emergency and PathCurrentNode 
    //
    // Using this, the last node of the return path is sufficient to be stored
    private LinkedListNode<Field>? ReturnPathLastNode { get; set; }

    private int ExtinguishCounter { get; set; }

    #endregion
    
    internal FireTruck(Field station)
    {
        Station = station;
        
        Active = false;
        Moving = false;
        DepartedFromStation = true;
        
        ExtinguishCounter = 0;
    }

    internal void Update()
    {
        if (!Active && !Moving)
            throw new Exception("Internal inconsistency: Attempted to update a fire truck that is neither active nor moving");

        switch (Moving, Active, RescueCounter: ExtinguishCounter)
        {
            case (true, true, _) when PathCurrentNode?.Next == null:
                // Arrived at the fire
                Moving = false;
                break;
            case (true, false, _) when PathCurrentNode?.Previous?.Value == Station:
                // Arrived at the station
                Moving = false;
                
                // Reset paths
                PathCurrentNode = null;
                ReturnPathLastNode = null;
                break;
            case (true, _, _):
                // On its way, either to the fire or back to the station
                // Step onto the next field
                // The next field is the next node in the path if the truck is on its way
                //  to the fire, or the previous node if it's on its way back
                PathCurrentNode = Active ? PathCurrentNode?.Next : PathCurrentNode?.Previous;
                break;
            case (false, true, < RescueCounterMax):
                // At the fire, still extinguishing
                ExtinguishCounter++;
                break;
            case (false, true, >= RescueCounterMax):
                // At the fire, extinguished
                Active = false;
                Moving = true;
                    
                // Turn back to the station
                PathCurrentNode = ReturnPathLastNode;
                break;
        }
    }

    internal void Deploy(LinkedList<Field> path, LinkedList<Field> returnPath)
    {
        if (Active)
            throw new Exception("Internal inconsistency: Attempted to assign a fire emergency to a fire truck that is currently active.");
        
        if (returnPath.Last?.Value != path.Last?.Value && returnPath.First?.Value != Station)
            throw new Exception("Internal inconsistency: Attempted to assign a path to a fire truck whose last stop is not the first stop of the return path. The return path should be a list whose **FIRST** item is the fire truck's home department, and last item is the location of the fire emergency that the fire truck is assigned to.");

        if (!Moving)
            DepartedFromStation = true;
        
        Active = true;
        Moving = true;
        
        ExtinguishCounter = 0;

        PathCurrentNode = path.First;
        ReturnPathLastNode = returnPath.Last;
    }

    internal void Cancel(LinkedList<Field>? returnPath)
    {
        if (!Active)
            throw new Exception("Internal inconsistency: Attempted to cancel a fire truck that is not active");
        
        if (returnPath != null)
            ReturnPathLastNode = returnPath.Last;

        Active = false;
        Moving = true;
    }
}