using Unity.Entities;
using UnityEngine;

public class NPCAuthoring : MonoBehaviour
{
    [Header("NPC Settings")]
    public float MaxSpeed = 1.5f;
    public float DetectionDistance = 3f;
    public float AvoidanceDistance = 1.5f;

    [Header("Route Waypoints")]
    public Transform Waypoint0;
    public Transform Waypoint1;
    public Transform Waypoint2;
    public Transform Waypoint3;

    [Header("Route")]
    public int RouteID = 0;

    [Header("Direction")]
    [Tooltip("Clockwise = 1, CounterClockwise = -1, Random = 0")]
    public int StartDirection = 0;  // 0 = random

    class Baker : Baker<NPCAuthoring>
    {
        public override void Bake(NPCAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.Dynamic);

            // Determinar dirección
            int direction = authoring.StartDirection;
            if (direction == 0)
            {
                direction = UnityEngine.Random.value > 0.5f ? 1 : -1;
            }

            AddComponent(entity, new NPC
            {
                Speed = authoring.MaxSpeed,
                MaxSpeed = authoring.MaxSpeed,
                CurrentWaypoint = 0,
                Direction = direction,
                DetectionDistance = authoring.DetectionDistance,
                AvoidanceDistance = authoring.AvoidanceDistance
            });

            AddComponent(entity, new NPCProgress
            {
                DistanceToNextWaypoint = 0
            });

            AddComponent(entity, new NPCWaypoints
            {
                Point0 = authoring.Waypoint0.position,
                Point1 = authoring.Waypoint1.position,
                Point2 = authoring.Waypoint2.position,
                Point3 = authoring.Waypoint3.position
            });

            AddComponent(entity, new NPCRouteID
            {
                ID = authoring.RouteID
            });

            AddComponent(entity, new NPCAnimationSpeed
            {
                AnimSpeed = 1f
            });
        }
    }
}
