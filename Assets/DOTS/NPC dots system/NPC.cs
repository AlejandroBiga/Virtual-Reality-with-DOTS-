using Unity.Entities;

public struct NPC : IComponentData
{
    public float Speed;
    public float MaxSpeed;
    public int CurrentWaypoint;    
    public int Direction;           
    public float DetectionDistance;
    public float AvoidanceDistance;
}
