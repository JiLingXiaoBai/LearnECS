using Unity.Entities;
using UnityEngine;

public class UnitAuthoring : MonoBehaviour
{
    private class Baker : Baker<UnitAuthoring>
    {
        public override void Bake(UnitAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(entity, new Unit());
        }
    }
}

public struct Unit : IComponentData
{
}