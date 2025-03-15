using Unity.Entities;
using UnityEngine;

public class LoseTargetAuthoring : MonoBehaviour
{
    public float LoseTargetDistance;

    private class Baker : Baker<LoseTargetAuthoring>
    {
        public override void Bake(LoseTargetAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(entity, new LoseTarget { loseTargetDistance = authoring.LoseTargetDistance });
        }
    }
}

public struct LoseTarget : IComponentData
{
    public float loseTargetDistance;
}