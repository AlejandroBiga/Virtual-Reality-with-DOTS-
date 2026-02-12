using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Collections;

partial struct VehicleInitSystem : ISystem
{
    private bool initialized;

    public void OnUpdate(ref SystemState state)
    {
        if (initialized) return;
        var vehicleQuery = SystemAPI.QueryBuilder()
            .WithAll<Vehicle, VehicleProgress, RouteWaypoints, RouteID>()
            .Build();

        if (vehicleQuery.IsEmpty) return;

        var entities = vehicleQuery.ToEntityArray(Allocator.Temp);
        var waypoints = vehicleQuery.ToComponentDataArray<RouteWaypoints>(Allocator.Temp);
        var routeIDs = vehicleQuery.ToComponentDataArray<RouteID>(Allocator.Temp);

        if (waypoints.Length > 0)
        {
            var wp = waypoints[0];
            float totalLength =
                math.distance(wp.Point0, wp.Point1) +
                math.distance(wp.Point1, wp.Point2) +
                math.distance(wp.Point2, wp.Point3) +
                math.distance(wp.Point3, wp.Point0);

            int count = entities.Length;
            float spacing = totalLength / count;

            for (int i = 0; i < count; i++)
            {
                float targetDistance = i * spacing;
                float seg0 = math.distance(wp.Point0, wp.Point1);
                float seg1 = math.distance(wp.Point1, wp.Point2);
                float seg2 = math.distance(wp.Point2, wp.Point3);

                int waypoint = 0;
                float distInSegment = targetDistance;

                if (targetDistance < seg0)
                {
                    waypoint = 0;
                    distInSegment = targetDistance;
                }
                else if (targetDistance < seg0 + seg1)
                {
                    waypoint = 1;
                    distInSegment = targetDistance - seg0;
                }
                else if (targetDistance < seg0 + seg1 + seg2)
                {
                    waypoint = 2;
                    distInSegment = targetDistance - seg0 - seg1;
                }
                else
                {
                    waypoint = 3;
                    distInSegment = targetDistance - seg0 - seg1 - seg2;
                }

                var vehicle = state.EntityManager.GetComponentData<Vehicle>(entities[i]);
                vehicle.CurrentWaypoint = waypoint;
                state.EntityManager.SetComponentData(entities[i], vehicle);

                state.EntityManager.SetComponentData(entities[i], new VehicleProgress
                {
                    DistanceToNextWaypoint = distInSegment
                });
            }
        }

        entities.Dispose();
        waypoints.Dispose();
        routeIDs.Dispose();

        initialized = true;
    }
}
