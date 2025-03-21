using Unity.Entities;
using UnityEngine;

public class BuildingBarracksAuthoring : MonoBehaviour
{
    public float progressMax;
    private class Baker : Baker<BuildingBarracksAuthoring>
    {
        public override void Bake(BuildingBarracksAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(entity, new BuildingBarracks
            {
                progressMax = authoring.progressMax,
            });
        }
    }
    
}

public struct BuildingBarracks : IComponentData
{
    public float progress;
    public float progressMax;
}
