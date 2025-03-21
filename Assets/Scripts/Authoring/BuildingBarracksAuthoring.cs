using Unity.Entities;
using Unity.Mathematics;
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
                rallyPositionOffset = new float3(10, 0, 0),
            });
            AddBuffer<SpawnUnitTypeBuffer>(entity);
        }
    }
}

public struct BuildingBarracks : IComponentData
{
    public float progress;
    public float progressMax;
    public UnitTypeSO.UnitType activeUnitType;
    public float3 rallyPositionOffset;
}

[InternalBufferCapacity(10)]
public struct SpawnUnitTypeBuffer : IBufferElementData
{
    public UnitTypeSO.UnitType unitType;
}