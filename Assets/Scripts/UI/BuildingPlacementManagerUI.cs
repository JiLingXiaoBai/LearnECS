using UnityEngine;

public class BuildingPlacementManagerUI : MonoBehaviour
{
    [SerializeField] private RectTransform buildingContainer;
    [SerializeField] private RectTransform buildingTemplate;
    [SerializeField] private BuildingTypeListSO buildingTypeListSO;

    private void Awake()
    {
        buildingTemplate.gameObject.SetActive(false);
        foreach (BuildingTypeSO buildingTypeSO in buildingTypeListSO.buildingTypeSOList)
        {
            RectTransform buildingRectTransform = Instantiate(buildingTemplate, buildingContainer);
            buildingRectTransform.gameObject.SetActive(true);
        }
    }
}
