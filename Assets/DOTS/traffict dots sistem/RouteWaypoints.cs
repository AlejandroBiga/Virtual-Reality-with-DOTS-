using Unity.Entities;
using Unity.Mathematics;

public struct RouteWaypoints : IComponentData
{
    public float3 Point0;
    public float3 Point1;
    public float3 Point2;
    public float3 Point3;
}
