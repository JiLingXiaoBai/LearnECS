using System.Collections.Generic;
using UnityEngine;

public class ResourceManager : MonoBehaviour
{
    public static ResourceManager Instance { get; private set; }

    [SerializeField] private ResourceTypeListSO resourceTypeListSO;

    private Dictionary<ResourceTypeSO.ResourceType, int> resourceTypeAmountDictionary;

    private void Awake()
    {
        Instance = this;

        resourceTypeAmountDictionary = new Dictionary<ResourceTypeSO.ResourceType, int>();

        foreach (ResourceTypeSO resourceTypeSO in resourceTypeListSO.resourceTypeSOList)
        {
            resourceTypeAmountDictionary[resourceTypeSO.resourceType] = 0;
        }
    }

    public void AddResourceAmount(ResourceTypeSO.ResourceType resourceType, int amount)
    {
        resourceTypeAmountDictionary[resourceType] += amount;
    }
    
    public int GetResourceAmount(ResourceTypeSO.ResourceType resourceType)
    {
        return resourceTypeAmountDictionary[resourceType];
    }
}