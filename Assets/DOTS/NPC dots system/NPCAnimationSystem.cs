using Unity.Entities;
using UnityEngine;
[RequireMatchingQueriesForUpdate]

public partial class NPCAnimationSystem : SystemBase
{
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