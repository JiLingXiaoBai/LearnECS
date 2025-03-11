using Unity.Entities;
using UnityEngine;

public class MoveSpeedAuthoring : MonoBehaviour
{
    public float value;

    private class MoveSpeedBaker : Baker<MoveSpeedAuthoring>
    {
        public override void Bake(MoveSpeedAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(entity, new MoveSpeed { value = authoring.value });
        }
    }
}

public struct MoveSpeed : IComponentData
{
    public float value;
}