using UnityEngine;

[CreateAssetMenu()]
public class UnitTypeSO : ScriptableObject
{
    public enum UnitType
    {
        None,
        Soldier,
        Scout,
        Zombie
    }
    
    public UnitType unitType;
}