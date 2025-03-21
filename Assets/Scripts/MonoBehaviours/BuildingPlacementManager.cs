using Unity.Collections;
using Unity.Entities;
using Unity.Physics;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.EventSystems;

public class BuildingPlacementManager : MonoBehaviour
{
    [SerializeField] private BuildingTypeSO buildingTypeSO;

    private void Update()
    {
        if (EventSystem.current.IsPointerOverGameObject())
        {
            return;
        }

        if (Input.GetMouseButtonDown(0))
        {
            if (CanPlaceBuilding())
            {
                Vector3 mouseWorldPosition = MouseWorldPosition.Instance.GetPosition();
                EntityManager entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;

                EntityQuery entityQuery = entityManager.CreateEntityQuery(typeof(EntitiesReferences));
                EntitiesReferences entitiesReferences = entityQuery.GetSingleton<EntitiesReferences>();

                Entity spawnedEntity = entityManager.Instantiate(entitiesReferences.buildingTowerPrefabEntity);

                entityManager.SetComponentData(spawnedEntity, LocalTransform.FromPosition(mouseWorldPosition));
            }
        }
    }

    private bool CanPlaceBuilding()
    {
        var mouseWorldPosition = MouseWorldPosition.Instance.GetPosition();
        var entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;

        EntityQuery entityQuery = entityManager.CreateEntityQuery(typeof(PhysicsWorldSingleton));
        PhysicsWorldSingleton physicsWorldSingleton = entityQuery.GetSingleton<PhysicsWorldSingleton>();
        CollisionWorld collisionWorld = physicsWorldSingleton.CollisionWorld;

        CollisionFilter collisionFilter = new CollisionFilter()
        {
            BelongsTo = ~0u,
            CollidesWith = 1u << GameAssets.BUILDINGS_LAYER,
            GroupIndex = 0,
        };

        float bonusExtents = 1.1f;
        NativeList<DistanceHit> distanceHitList = new NativeList<DistanceHit>(Allocator.Temp);
        UnityEngine.BoxCollider boxCollider = buildingTypeSO.prefab.GetComponent<UnityEngine.BoxCollider>();
        if (collisionWorld.OverlapBox(mouseWorldPosition, Quaternion.identity, boxCollider.size * 0.5f * bonusExtents,
                ref distanceHitList, collisionFilter))
        {
            return false;
        }

        distanceHitList.Clear();
        if (collisionWorld.OverlapSphere(mouseWorldPosition, buildingTypeSO.buildingDistanceMin, ref distanceHitList,
                collisionFilter))
        {
            foreach (DistanceHit distanceHit in distanceHitList)
            {
                if (entityManager.HasComponent<BuildingTypeSOHolder>(distanceHit.Entity))
                {
                    BuildingTypeSOHolder buildingTypeSOHolder =
                        entityManager.GetComponentData<BuildingTypeSOHolder>(distanceHit.Entity);

                    if (buildingTypeSOHolder.buildingType == buildingTypeSO.buildingType)
                    {
                        return false;
                    }
                }
            }
        }


        return true;
    }
}