using Unity.Entities;
using UnityEngine;

public class BuildingConstructionAuthoring : MonoBehaviour
{
    private class Baker : Baker<BuildingConstructionAuthoring>
    {
        public override void Bake(BuildingConstructionAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(entity, new BuildingConstruction());
        }
    }
}

public struct BuildingConstruction : IComponentData
{
    public float constructionTimer;
    public float constructionTimerMax;
    public BuildingTypeSO.BuildingType buildingType;
    public Entity finalPrefabEntity;
    public Entity visualEntity;
}