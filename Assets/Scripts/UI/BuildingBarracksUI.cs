using System;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;
using UnityEngine.UI;

public class BuildingBarracksUI : MonoBehaviour
{
    [SerializeField] private Button soldierButton;
    [SerializeField] private Image progressBarImage;

    private Entity buildingBarracksEntity;
    private EntityManager entityManager;

    private void Awake()
    {
        soldierButton.onClick.AddListener(() =>
        {
            DynamicBuffer<SpawnUnitTypeBuffer> spawnUnitTypeDynamicBuffer =
                entityManager.GetBuffer<SpawnUnitTypeBuffer>(buildingBarracksEntity, false);
            spawnUnitTypeDynamicBuffer.Add(new SpawnUnitTypeBuffer
            {
                unitType = UnitTypeSO.UnitType.Soldier,
            });
        });
    }

    private void Start()
    {
        entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
        UnitSelectionManager.Instance.OnSelectedEntitiesChanged += UnitSelectionManager_OnSelectedEntitiesChanged;
        Hide();
    }

    private void UnitSelectionManager_OnSelectedEntitiesChanged(object sender, System.EventArgs e)
    {
        EntityQuery entityQuery = new EntityQueryBuilder(Allocator.Temp).WithAll<Selected, BuildingBarracks>()
            .Build(entityManager);
        NativeArray<Entity> entityArray = entityQuery.ToEntityArray(Allocator.Temp);
        if (entityArray.Length > 0)
        {
            buildingBarracksEntity = entityArray[0];
            Show();
        }
        else
        {
            buildingBarracksEntity = Entity.Null;
            Hide();
        }
    }

    private void UpdateProgressBarVisual()
    {
        if (buildingBarracksEntity == Entity.Null)
        {
            progressBarImage.fillAmount = 0;
            return;
        }

        BuildingBarracks buildingBarracks = entityManager.GetComponentData<BuildingBarracks>(buildingBarracksEntity);

        if (buildingBarracks.activeUnitType == UnitTypeSO.UnitType.None)
        {
            progressBarImage.fillAmount = 0;
        }
        else
        {
            progressBarImage.fillAmount = buildingBarracks.progress / buildingBarracks.progressMax;
        }
    }

    private void Show()
    {
        gameObject.SetActive(true);
    }

    private void Hide()
    {
        gameObject.SetActive(false);
    }
}