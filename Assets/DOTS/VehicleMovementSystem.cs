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
            .WithAll<Vehicle, VehicleProgress, LocalTransform, RouteWaypoints>()
            .Build();

        var allVehicles = vehicleQuery.ToComponentDataArray<Vehicle>(Allocator.TempJob);
        var allProgress = vehicleQuery.ToComponentDataArray<VehicleProgress>(Allocator.TempJob);
        var allTransforms = vehicleQuery.ToComponentDataArray<LocalTransform>(Allocator.TempJob);
        var allWaypoints = vehicleQuery.ToComponentDataArray<RouteWaypoints>(Allocator.TempJob);

        new VehicleJob
        {
            DeltaTime = deltaTime,
            AllVehicles = allVehicles,
            AllProgress = allProgress,
            AllTransforms = allTransforms,
            AllWaypoints = allWaypoints
        }.ScheduleParallel();

        state.Dependency.Complete();
        allVehicles.Dispose();
        allProgress.Dispose();
        allTransforms.Dispose();
        allWaypoints.Dispose();
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

    void Execute(
        ref LocalTransform transform,
        ref VehicleProgress progress,
        ref Vehicle vehicle,
        in RouteWaypoints waypoints,
        [EntityIndexInQuery] int entityIndex)
    {
        
        float3 currentWP = GetWaypoint(waypoints, vehicle.CurrentWaypoint);
        float3 nextWP = GetWaypoint(waypoints, (vehicle.CurrentWaypoint + 1) % 4);

        float3 direction = math.normalize(nextWP - currentWP);
        float segmentLength = math.distance(currentWP, nextWP);

        float currentSpeed = vehicle.MaxSpeed;

        for (int i = 0; i < AllVehicles.Length; i++)
        {
            if (i == entityIndex) continue;

            var other = AllVehicles[i];
            var otherProgress = AllProgress[i];
            var otherTransform = AllTransforms[i];
            var otherWaypoints = AllWaypoints[i];

            if (!math.all(otherWaypoints.Point0 == waypoints.Point0)) continue;

            if (other.CurrentWaypoint == vehicle.CurrentWaypoint)
            {
                float distAhead = otherProgress.DistanceToNextWaypoint - progress.DistanceToNextWaypoint;

                if (distAhead > 0 && distAhead < vehicle.DetectionDistance)
                {
                    if (distAhead < vehicle.StopDistance)
                    {
                        currentSpeed = 0;  
                    }
                    else
                    {
                       
                        currentSpeed = vehicle.MaxSpeed * (distAhead / vehicle.DetectionDistance);
                    }
                    break;
                }
            }
            
            else if (other.CurrentWaypoint == (vehicle.CurrentWaypoint + 1) % 4)
            {
                float distToEnd = segmentLength - progress.DistanceToNextWaypoint;
                float totalDist = distToEnd + otherProgress.DistanceToNextWaypoint;

                if (totalDist < vehicle.DetectionDistance)
                {
                    if (totalDist < vehicle.StopDistance)
                    {
                        currentSpeed = 0;
                    }
                    else
                    {
                        currentSpeed = vehicle.MaxSpeed * (totalDist / vehicle.DetectionDistance);
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
