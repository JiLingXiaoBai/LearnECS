using Unity.Burst;
using Unity.Entities;
using Unity.Transforms;

partial struct BuildingBarracksSystem : ISystem
{
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<EntitiesReferences>();
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        EntitiesReferences entitiesReferences = SystemAPI.GetSingleton<EntitiesReferences>();
        foreach ((RefRO<LocalTransform> localTransform, RefRW<BuildingBarracks> buildingBarracks) in SystemAPI
                     .Query<RefRO<LocalTransform>, RefRW<BuildingBarracks>>())
        {
            buildingBarracks.ValueRW.progress += SystemAPI.Time.DeltaTime;
            if (buildingBarracks.ValueRO.progress < buildingBarracks.ValueRO.progressMax)
            {
                continue;
            }
            buildingBarracks.ValueRW.progress = 0;

            Entity spawnedUnitEntity = state.EntityManager.Instantiate(entitiesReferences.soldierPrefabEntity);
            SystemAPI.SetComponent(spawnedUnitEntity, LocalTransform.FromPosition(localTransform.ValueRO.Position));
            
        }
    }
}