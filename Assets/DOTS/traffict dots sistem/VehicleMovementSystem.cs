using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

partial struct VehicleMovementSystem : ISystem
{
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        float deltaTime = SystemAPI.Time.DeltaTime;

        var vehicleQuery = SystemAPI.QueryBuilder()
            .WithAll<Vehicle, VehicleProgress, LocalTransform, RouteWaypoints, RouteID>()
            .Build();

        var allVehicles = vehicleQuery.ToComponentDataArray<Vehicle>(Allocator.TempJob);
        var allProgress = vehicleQuery.ToComponentDataArray<VehicleProgress>(Allocator.TempJob);
        var allTransforms = vehicleQuery.ToComponentDataArray<LocalTransform>(Allocator.TempJob);
        var allWaypoints = vehicleQuery.ToComponentDataArray<RouteWaypoints>(Allocator.TempJob);
        var allRouteIDs = vehicleQuery.ToComponentDataArray<RouteID>(Allocator.TempJob);

        new VehicleJob
        {
            DeltaTime = deltaTime,
            AllVehicles = allVehicles,
            AllProgress = allProgress,
            AllTransforms = allTransforms,
            AllWaypoints = allWaypoints,
            AllRouteIDs = allRouteIDs
        }.ScheduleParallel();

        state.Dependency.Complete();
        allVehicles.Dispose();
        allProgress.Dispose();
        allTransforms.Dispose();
        allWaypoints.Dispose();
        allRouteIDs.Dispose();
    }
}

[BurstCompile]
partial struct VehicleJob : IJobEntity
{
    public float DeltaTime;

    [ReadOnly] public NativeArray<Vehicle> AllVehicles;
    [ReadOnly] public NativeArray<VehicleProgress> AllProgress;
    [ReadOnly] public NativeArray<LocalTransform> AllTransforms;
    [ReadOnly] public NativeArray<RouteWaypoints> AllWaypoints;
    [ReadOnly] public NativeArray<RouteID> AllRouteIDs;

    void Execute(
        ref LocalTransform transform,
        ref VehicleProgress progress,
        ref Vehicle vehicle,
        in RouteWaypoints waypoints,
        in RouteID routeID,
        [EntityIndexInQuery] int entityIndex)
    {
        float3 currentWP = GetWaypoint(waypoints, vehicle.CurrentWaypoint);
        float3 nextWP = GetWaypoint(waypoints, (vehicle.CurrentWaypoint + 1) % 4);

        float3 direction = math.normalize(nextWP - currentWP);
        float segmentLength = math.distance(currentWP, nextWP);
        float currentSpeed = vehicle.MaxSpeed;
        float3 myPosition = transform.Position;
        float3 myForward = math.normalize(math.rotate(transform.Rotation, new float3(0, 0, 1)));

        for (int i = 0; i < AllVehicles.Length; i++)
        {
            if (i == entityIndex) continue;

            var otherRouteID = AllRouteIDs[i];
            if (otherRouteID.ID != routeID.ID) continue;
            var otherTransform = AllTransforms[i];
            float3 otherPosition = otherTransform.Position;

            float3 toOther = otherPosition - myPosition;
            float distance = math.length(toOther);

            if (distance < vehicle.DetectionDistance && distance > 0.01f)
            {
                float3 toOtherNorm = math.normalize(toOther);
                float dotProduct = math.dot(myForward, toOtherNorm);

                if (dotProduct > 0.5f)
                {
                    if (distance < vehicle.StopDistance)
                    {
                        currentSpeed = 0;  
                    }
                    else
                    {
                        float slowFactor = (distance - vehicle.StopDistance) /
                                         (vehicle.DetectionDistance - vehicle.StopDistance);
                        slowFactor = math.clamp(slowFactor, 0f, 1f);
                        currentSpeed = vehicle.MaxSpeed * slowFactor;
                    }
                    break;
                }
            }
        }

        vehicle.Speed = currentSpeed;

        progress.DistanceToNextWaypoint += vehicle.Speed * DeltaTime;

        if (progress.DistanceToNextWaypoint >= segmentLength)
        {
            progress.DistanceToNextWaypoint = 0;
            vehicle.CurrentWaypoint = (vehicle.CurrentWaypoint + 1) % 4;
            currentWP = GetWaypoint(waypoints, vehicle.CurrentWaypoint);
            nextWP = GetWaypoint(waypoints, (vehicle.CurrentWaypoint + 1) % 4);
            direction = math.normalize(nextWP - currentWP);
            segmentLength = math.distance(currentWP, nextWP);
        }

        float t = progress.DistanceToNextWaypoint / segmentLength;
        transform.Position = math.lerp(currentWP, nextWP, t);
        transform.Rotation = quaternion.LookRotationSafe(direction, math.up());
    }

    float3 GetWaypoint(RouteWaypoints waypoints, int index)
    {
        switch (index)
        {
            case 0: return waypoints.Point0;
            case 1: return waypoints.Point1;
            case 2: return waypoints.Point2;
            case 3: return waypoints.Point3;
            default: return waypoints.Point0;
        }
    }
}