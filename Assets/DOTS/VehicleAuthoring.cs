using Unity.Entities;
using Unity.Mathematics;
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

    [Header("Starting Position")]
    [Range(0, 3)] public int StartingWaypoint = 0;  // En qué waypoint empieza (0, 1, 2, 3)
    [Range(0f, 1f)] public float StartingProgress = 0f;  // Qué tan avanzado en ese segmento (0 = inicio, 1 = final)

    class Baker : Baker<VehicleAuthoring>
    {
        public override void Bake(VehicleAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.Dynamic);

            // Calcular distancia del segmento inicial
            float3 currentWP = GetWaypointPosition(authoring, authoring.StartingWaypoint);
            float3 nextWP = GetWaypointPosition(authoring, (authoring.StartingWaypoint + 1) % 4);
            float segmentLength = math.distance(currentWP, nextWP);

            AddComponent(entity, new Vehicle
            {
                Speed = authoring.MaxSpeed,
                MaxSpeed = authoring.MaxSpeed,
                CurrentWaypoint = authoring.StartingWaypoint,
                DetectionDistance = authoring.DetectionDistance,
                StopDistance = authoring.StopDistance
            });

            AddComponent(entity, new VehicleProgress
            {
                DistanceToNextWaypoint = authoring.StartingProgress * segmentLength
            });

            AddComponent(entity, new RouteWaypoints
            {
                Point0 = authoring.Waypoint0.position,
                Point1 = authoring.Waypoint1.position,
                Point2 = authoring.Waypoint2.position,
                Point3 = authoring.Waypoint3.position
            });
        }

        static float3 GetWaypointPosition(VehicleAuthoring authoring, int index)
        {
            switch (index)
            {
                case 0: return authoring.Waypoint0.position;
                case 1: return authoring.Waypoint1.position;
                case 2: return authoring.Waypoint2.position;
                case 3: return authoring.Waypoint3.position;
                default: return authoring.Waypoint0.position;
            }
        }
    }
}
