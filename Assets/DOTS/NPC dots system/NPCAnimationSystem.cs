using Unity.Entities;
using UnityEngine;

[RequireMatchingQueriesForUpdate]
[WorldSystemFilter(WorldSystemFilterFlags.Default | WorldSystemFilterFlags.Editor)]
// AGREGAR ESTA LÍNEA:
[UpdateInGroup(typeof(SimulationSystemGroup))]
public partial class NPCAnimationSystem : SystemBase
{
    protected override void OnCreate()
    {
        // Registrar el tipo Animator para que DOTS lo reconozca
        base.OnCreate();
    }

    protected override void OnUpdate()
    {
        foreach (var (animator, animSpeed) in
                 SystemAPI.Query<SystemAPI.ManagedAPI.UnityEngineComponent<Animator>,
                                 RefRO<NPCAnimationSpeed>>())
        {
            if (animator.Value != null && animator.Value.isActiveAndEnabled)
            {
                animator.Value.speed = animSpeed.ValueRO.AnimSpeed;
            }
        }
    }
}