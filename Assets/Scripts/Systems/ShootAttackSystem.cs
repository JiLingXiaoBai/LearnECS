using Unity.Burst;
using Unity.Entities;

partial struct ShootAttackSystem : ISystem
{
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        foreach ((RefRW<ShootAttack> shootAttack, RefRO<Target> target) in SystemAPI
                     .Query<RefRW<ShootAttack>, RefRO<Target>>())
        {
            if (target.ValueRO.targetEntity == Entity.Null)
            {
                continue;
            }
            shootAttack.ValueRW.timer -= SystemAPI.Time.DeltaTime;
            if (shootAttack.ValueRW.timer > 0)
            {
                continue;
            }
            shootAttack.ValueRW.timer = shootAttack.ValueRW.timerMax;
            RefRW<Health> targetHealth = SystemAPI.GetComponentRW<Health>(target.ValueRO.targetEntity);
            int damageAmount = 1;
            targetHealth.ValueRW.healthAmount -= damageAmount;
        }
    }
}