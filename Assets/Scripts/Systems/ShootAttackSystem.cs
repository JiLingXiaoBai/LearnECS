using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

partial struct ShootAttackSystem : ISystem
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

        foreach ((RefRW<LocalTransform> localTransform, RefRW<ShootAttack> shootAttack,
                     RefRW<TargetPositionPathQueued> targetPositionPathQueued,
                     EnabledRefRW<TargetPositionPathQueued> targetPositionPathQueuedEnabled,
                     RefRO<Target> target, RefRW<UnitMover> unitMover, Entity entity) in SystemAPI
                     .Query<RefRW<LocalTransform>, RefRW<ShootAttack>, RefRW<TargetPositionPathQueued>,
                         EnabledRefRW<TargetPositionPathQueued>, RefRO<Target>, RefRW<UnitMover>>()
                     .WithDisabled<MoveOverride>().WithPresent<TargetPositionPathQueued>().WithEntityAccess())
        {
            if (target.ValueRO.targetEntity == Entity.Null)
            {
                continue;
            }

            LocalTransform targetLocalTransform = SystemAPI.GetComponent<LocalTransform>(target.ValueRO.targetEntity);

            if (math.distancesq(localTransform.ValueRO.Position, targetLocalTransform.Position) >
                math.pow(shootAttack.ValueRO.attackDistance, 2))
            {
                //Not close enough, start moving towards target
                targetPositionPathQueued.ValueRW.targetPosition = targetLocalTransform.Position;
                targetPositionPathQueuedEnabled.ValueRW = true;
                continue;
            }
            else
            {
                //Close enough, stop moving and attack
                targetPositionPathQueued.ValueRW.targetPosition = localTransform.ValueRO.Position;
                targetPositionPathQueuedEnabled.ValueRW = true;
            }

            float3 aimDirection = targetLocalTransform.Position - localTransform.ValueRO.Position;
            aimDirection = math.normalize(aimDirection);
            quaternion targetRotation = quaternion.LookRotation(aimDirection, math.up());
            localTransform.ValueRW.Rotation = math.slerp(localTransform.ValueRO.Rotation, targetRotation,
                unitMover.ValueRO.rotationSpeed * SystemAPI.Time.DeltaTime);
        }

        foreach ((RefRW<LocalTransform> localTransform, RefRW<ShootAttack> shootAttack,
                     RefRO<Target> target, Entity entity) in SystemAPI
                     .Query<RefRW<LocalTransform>, RefRW<ShootAttack>, RefRO<Target>>()
                     .WithEntityAccess())
        {
            if (target.ValueRO.targetEntity == Entity.Null)
            {
                continue;
            }

            LocalTransform targetLocalTransform = SystemAPI.GetComponent<LocalTransform>(target.ValueRO.targetEntity);
            if (math.distancesq(localTransform.ValueRO.Position, targetLocalTransform.Position) >
                math.pow(shootAttack.ValueRO.attackDistance, 2))
            {
                continue;
            }

            if (SystemAPI.HasComponent<MoveOverride>(entity) && SystemAPI.IsComponentEnabled<MoveOverride>(entity))
            {
                continue;
            }

            shootAttack.ValueRW.timer -= SystemAPI.Time.DeltaTime;
            if (shootAttack.ValueRW.timer > 0)
            {
                continue;
            }
            shootAttack.ValueRW.timer = shootAttack.ValueRW.timerMax;

            if (SystemAPI.HasComponent<TargetOverride>(target.ValueRO.targetEntity))
            {
                RefRW<TargetOverride> enemyTargetOverride =
                    SystemAPI.GetComponentRW<TargetOverride>(target.ValueRO.targetEntity);
                if (enemyTargetOverride.ValueRO.targetEntity == Entity.Null)
                {
                    enemyTargetOverride.ValueRW.targetEntity = entity;
                }
            }

            Entity bulletEntity = state.EntityManager.Instantiate(entitiesReferences.bulletPrefabEntity);
            float3 bulletSpawnWorldPosition =
                localTransform.ValueRO.TransformPoint(shootAttack.ValueRO.bulletSpawnLocalPosition);

            SystemAPI.SetComponent(bulletEntity, LocalTransform.FromPosition(bulletSpawnWorldPosition));

            RefRW<Bullet> bulletBullet = SystemAPI.GetComponentRW<Bullet>(bulletEntity);
            bulletBullet.ValueRW.damageAmount = shootAttack.ValueRO.damageAmount;

            RefRW<Target> bulletTarget = SystemAPI.GetComponentRW<Target>(bulletEntity);
            bulletTarget.ValueRW.targetEntity = target.ValueRO.targetEntity;

            shootAttack.ValueRW.onShoot.isTriggered = true;
            shootAttack.ValueRW.onShoot.shootFromPosition = bulletSpawnWorldPosition;
        }
    }
}