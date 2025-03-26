using UnityEngine;

public class GameAssets : MonoBehaviour
{
    public const int UNITS_LAYER = 6;
    public const int BUILDINGS_LAYER = 7;
    public const int PATHFINDING_WALLS = 8;
    public const int PATHFINDING_HEAVY = 9;
    public static GameAssets Instance { get; private set; }

    private void Awake()
    {
        Instance = this;
    }

    public UnitTypeListSO unitTypeListSO;
    public BuildingTypeListSO buildingTypeListSO;
}