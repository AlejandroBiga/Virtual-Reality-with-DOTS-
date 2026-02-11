using Unity.Entities;

public struct Vehicle : IComponentData
{
    public float Speed;
    public float MaxSpeed;
    public int CurrentWaypoint; 
    public float DetectionDistance;
    public float StopDistance;
}
