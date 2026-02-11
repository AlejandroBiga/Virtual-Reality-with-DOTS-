using Unity.Entities;
using UnityEngine;

public class VehicleAuthoring : MonoBehaviour
{
    [Header("Vehicle Settings")]
    public float MaxSpeed = 5f;
    public float DetectionDistance = 5f;
    public float StopDistance = 2f;

    [Header("Route Waypoints")]
    public Transform Waypoint0;
    public Transform Waypoint1;
    public Transform Waypoint2;
    public Transform Waypoint3;

    class Baker : Baker<VehicleAuthoring>
    {
        public override void Bake(VehicleAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.Dynamic);

            AddComponent(entity, new Vehicle
            {
                Speed = authoring.MaxSpeed,
                MaxSpeed = authoring.MaxSpeed,
                CurrentWaypoint = 0,
                DetectionDistance = authoring.DetectionDistance,
                StopDistance = authoring.StopDistance
            });

            AddComponent(entity, new VehicleProgress
            {
                DistanceToNextWaypoint = 0
            });

            AddComponent(entity, new RouteWaypoints
            {
                Point0 = authoring.Waypoint0.position,
                Point1 = authoring.Waypoint1.position,
                Point2 = authoring.Waypoint2.position,
                Point3 = authoring.Waypoint3.position
            });
        }
    }
}
