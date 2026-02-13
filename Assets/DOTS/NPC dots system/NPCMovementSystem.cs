using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

partial struct NPCMovementSystem : ISystem
{
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        float deltaTime = SystemAPI.Time.DeltaTime;

        var npcQuery = SystemAPI.QueryBuilder()
            .WithAll<NPC, NPCProgress, LocalTransform, NPCWaypoints, NPCRouteID>()
            .Build();

        var allNPCs = npcQuery.ToComponentDataArray<NPC>(Allocator.TempJob);
        var allProgress = npcQuery.ToComponentDataArray<NPCProgress>(Allocator.TempJob);
        var allTransforms = npcQuery.ToComponentDataArray<LocalTransform>(Allocator.TempJob);
        var allWaypoints = npcQuery.ToComponentDataArray<NPCWaypoints>(Allocator.TempJob);
        var allRouteIDs = npcQuery.ToComponentDataArray<NPCRouteID>(Allocator.TempJob);

        new NPCMovementJob
        {
            DeltaTime = deltaTime,
            AllNPCs = allNPCs,
            AllProgress = allProgress,
            AllTransforms = allTransforms,
            AllWaypoints = allWaypoints,
            AllRouteIDs = allRouteIDs
        }.ScheduleParallel();

        state.Dependency.Complete();
        allNPCs.Dispose();
        allProgress.Dispose();
        allTransforms.Dispose();
        allWaypoints.Dispose();
        allRouteIDs.Dispose();
    }
}

[BurstCompile]
partial struct NPCMovementJob : IJobEntity
{
    public float DeltaTime;

    [ReadOnly] public NativeArray<NPC> AllNPCs;
    [ReadOnly] public NativeArray<NPCProgress> AllProgress;
    [ReadOnly] public NativeArray<LocalTransform> AllTransforms;
    [ReadOnly] public NativeArray<NPCWaypoints> AllWaypoints;
    [ReadOnly] public NativeArray<NPCRouteID> AllRouteIDs;

    void Execute(
        ref LocalTransform transform,
        ref NPCProgress progress,
        ref NPC npc,
        ref NPCAnimationSpeed animSpeed,
        in NPCWaypoints waypoints,
        in NPCRouteID routeID,
        [EntityIndexInQuery] int entityIndex)
    {
        // Obtener waypoint actual y siguiente según dirección
        int nextWaypointIndex = (npc.CurrentWaypoint + npc.Direction + 4) % 4;

        float3 currentWP = GetWaypoint(waypoints, npc.CurrentWaypoint);
        float3 nextWP = GetWaypoint(waypoints, nextWaypointIndex);

        float3 direction = math.normalize(nextWP - currentWP);
        float segmentLength = math.distance(currentWP, nextWP);

        // DETECCIÓN: Esquivar otros NPCs
        float currentSpeed = npc.MaxSpeed;
        float3 myPosition = transform.Position;
        float3 myForward = math.normalize(math.rotate(transform.Rotation, new float3(0, 0, 1)));

        bool shouldAvoid = false;
        float3 avoidanceDirection = float3.zero;

        for (int i = 0; i < AllNPCs.Length; i++)
        {
            if (i == entityIndex) continue;

            var otherRouteID = AllRouteIDs[i];
            if (otherRouteID.ID != routeID.ID) continue;

            var otherTransform = AllTransforms[i];
            float3 otherPosition = otherTransform.Position;

            float3 toOther = otherPosition - myPosition;
            float distance = math.length(toOther);

            if (distance < npc.DetectionDistance && distance > 0.01f)
            {
                float3 toOtherNorm = math.normalize(toOther);
                float dotProduct = math.dot(myForward, toOtherNorm);

                // Si está adelante
                if (dotProduct > 0.3f)
                {
                    if (distance < npc.AvoidanceDistance)
                    {
                        // MUY cerca - frenar y esquivar
                        currentSpeed = 0;

                        // Calcular dirección de esquive (perpendicular)
                        float3 perpendicular = new float3(-toOtherNorm.z, 0, toOtherNorm.x);
                        avoidanceDirection += perpendicular;
                        shouldAvoid = true;
                    }
                    else
                    {
                        // Cerca - solo reducir velocidad
                        float slowFactor = (distance - npc.AvoidanceDistance) /
                                         (npc.DetectionDistance - npc.AvoidanceDistance);
                        slowFactor = math.clamp(slowFactor, 0.3f, 1f);
                        currentSpeed = npc.MaxSpeed * slowFactor;
                    }
                }
            }
        }

        npc.Speed = currentSpeed;

        // MOVIMIENTO
        progress.DistanceToNextWaypoint += npc.Speed * DeltaTime;

        // Si llegó al waypoint, pasar al siguiente
        if (progress.DistanceToNextWaypoint >= segmentLength)
        {
            progress.DistanceToNextWaypoint = 0;
            npc.CurrentWaypoint = nextWaypointIndex;

            // Actualizar para próxima iteración
            currentWP = GetWaypoint(waypoints, npc.CurrentWaypoint);
            nextWaypointIndex = (npc.CurrentWaypoint + npc.Direction + 4) % 4;
            nextWP = GetWaypoint(waypoints, nextWaypointIndex);
            direction = math.normalize(nextWP - currentWP);
            segmentLength = math.distance(currentWP, nextWP);
        }

        // ACTUALIZAR POSICIÓN
        float t = progress.DistanceToNextWaypoint / segmentLength;
        float3 newPosition = math.lerp(currentWP, nextWP, t);

        // Aplicar esquive si es necesario
        if (shouldAvoid)
        {
            avoidanceDirection = math.normalize(avoidanceDirection);
            newPosition += avoidanceDirection * 0.3f * DeltaTime;  // Pequeño offset lateral
        }

        transform.Position = newPosition;

        // Rotar hacia la dirección de movimiento
        float3 lookDirection = direction;
        if (shouldAvoid)
        {
            lookDirection = math.normalize(direction + avoidanceDirection * 0.5f);
        }

        transform.Rotation = quaternion.LookRotationSafe(lookDirection, math.up());

        // Actualizar velocidad de animación
        animSpeed.AnimSpeed = npc.Speed / npc.MaxSpeed;
    }

    float3 GetWaypoint(NPCWaypoints waypoints, int index)
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
