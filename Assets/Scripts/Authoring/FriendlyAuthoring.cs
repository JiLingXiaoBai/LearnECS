using Unity.Entities;
using UnityEngine;

public class FriendlyAuthoring : MonoBehaviour
{
    private class Baker : Baker<FriendlyAuthoring>
    {
        public override void Bake(FriendlyAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(entity, new Friendly());
        }
    }
}

public struct Friendly : IComponentData
{
}