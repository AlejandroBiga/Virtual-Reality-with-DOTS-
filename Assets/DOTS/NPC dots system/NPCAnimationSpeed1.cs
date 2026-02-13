using Unity.Entities;
using UnityEngine;

public struct NPCAnimationSpeed1 : IComponentData
{
    public float AnimSpeed;
}

public class NPCAnimationBaker : Baker<Animator>
{
    public override void Bake(Animator authoring)
    {
        var entity = GetEntity(TransformUsageFlags.Dynamic);

        AddComponent(entity, new NPCAnimationSpeed
        {
            AnimSpeed = authoring.speed
        });

        AddComponentObject(entity, authoring);
    }
}