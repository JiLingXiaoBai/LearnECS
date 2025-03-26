using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Transforms;
using Unity.Mathematics;
using Unity.Physics;

partial struct UnitMoverSystem : ISystem
{
    public const float REACHED_TARGET_POSITION_DISTANCE_SQ = 2f;

    public ComponentLookup<TargetPositionPathQueued> targetPositionPathQueuedComponentLookup;
    public ComponentLookup<FlowFieldPathRequest> flowFieldPathRequestComponentLookup;
    public ComponentLookup<FlowFieldFollower> flowFieldFollowerComponentLookup;
    public ComponentLookup<MoveOverride> moveOverrideComponentLookup;

    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<GridSystem.GridSystemData>();
        targetPositionPathQueuedComponentLookup = SystemAPI.GetComponentLookup<TargetPositionPathQueued>(false);
        flowFieldPathRequestComponentLookup = SystemAPI.GetComponentLookup<FlowFieldPathRequest>(false);
        flowFieldFollowerComponentLookup = SystemAPI.GetComponentLookup<FlowFieldFollower>(false);
        moveOverrideComponentLookup = SystemAPI.GetComponentLookup<MoveOverride>(false);
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        GridSystem.GridSystemData gridSystemData = SystemAPI.GetSingleton<GridSystem.GridSystemData>();

        targetPositionPathQueuedComponentLookup.Update(ref state);
        flowFieldPathRequestComponentLookup.Update(ref state);
        flowFieldFollowerComponentLookup.Update(ref state);
        moveOverrideComponentLookup.Update(ref state);

        PhysicsWorldSingleton physicsWorldSingleton = SystemAPI.GetSingleton<PhysicsWorldSingleton>();
        CollisionWorld collisionWorld = physicsWorldSingleton.CollisionWorld;

        TargetPositionPathQueuedJob targetPositionPathQueuedJob = new TargetPositionPathQueuedJob
        {
            collisionWorld = collisionWorld,
            gridNodeSize = gridSystemData.gridNodeSize,
            width = gridSystemData.width,
            height = gridSystemData.height,
            costMap = gridSystemData.costMap,
            flowFieldFollowerComponentLookup = flowFieldFollowerComponentLookup,
            flowFieldPathRequestComponentLookup = flowFieldPathRequestComponentLookup,
            moveOverrideComponentLookup = moveOverrideComponentLookup,
            targetPositionPathQueuedComponentLookup = targetPositionPathQueuedComponentLookup,
        };
        JobHandle targetPositionPathQueuedJobHandle = targetPositionPathQueuedJob.ScheduleParallel(state.Dependency);
        targetPositionPathQueuedJobHandle.Complete();

        // foreach ((RefRO<LocalTransform> localTransform, RefRW<TargetPositionPathQueued> targetPositionPathQueued,
        //              EnabledRefRW<TargetPositionPathQueued> targetPositionPathQueuedEnabled,
        //              RefRW<FlowFieldPathRequest> flowFieldPathRequest,
        //              EnabledRefRW<FlowFieldPathRequest> flowFieldPathRequestEnabled,
        //              EnabledRefRW<FlowFieldFollower> flowFieldFollowerEnabled,
        //              RefRW<UnitMover> unitMover, Entity entity) in SystemAPI
        //              .Query<RefRO<LocalTransform>, RefRW<TargetPositionPathQueued>,
        //                  EnabledRefRW<TargetPositionPathQueued>, RefRW<FlowFieldPathRequest>,
        //                  EnabledRefRW<FlowFieldPathRequest>, EnabledRefRW<FlowFieldFollower>, RefRW<UnitMover>>()
        //              .WithPresent<FlowFieldPathRequest, FlowFieldFollower>().WithEntityAccess())
        // {
        //     RaycastInput raycastInput = new RaycastInput
        //     {
        //         Start = localTransform.ValueRO.Position,
        //         End = targetPositionPathQueued.ValueRO.targetPosition,
        //         Filter = new CollisionFilter
        //         {
        //             BelongsTo = ~0u,
        //             CollidesWith = 1u << GameAssets.PATHFINDING_WALLS,
        //             GroupIndex = 0
        //         }
        //     };
        //
        //     if (!collisionWorld.CastRay(raycastInput))
        //     {
        //         // Did not hit anything, no wall in between
        //         unitMover.ValueRW.targetPosition = targetPositionPathQueued.ValueRO.targetPosition;
        //         flowFieldPathRequestEnabled.ValueRW = false;
        //         flowFieldFollowerEnabled.ValueRW = false;
        //     }
        //     else
        //     {
        //         // There is a wall in between
        //         if (SystemAPI.HasComponent<MoveOverride>(entity))
        //         {
        //             SystemAPI.SetComponentEnabled<MoveOverride>(entity, false);
        //         }
        //         if (GridSystem.IsValidWalkableGridPosition(targetPositionPathQueued.ValueRO.targetPosition,
        //                 gridSystemData))
        //         {
        //             flowFieldPathRequest.ValueRW.targetPosition = targetPositionPathQueued.ValueRO.targetPosition;
        //             flowFieldPathRequestEnabled.ValueRW = true;
        //         }
        //         else
        //         {
        //             unitMover.ValueRW.targetPosition = localTransform.ValueRO.Position;
        //             flowFieldPathRequestEnabled.ValueRW = false;
        //             flowFieldFollowerEnabled.ValueRW = false;
        //         }
        //     }
        //     targetPositionPathQueuedEnabled.ValueRW = false;
        // }

        foreach ((RefRO<LocalTransform> localTransform, RefRW<FlowFieldFollower> flowFieldFollower,
                     EnabledRefRW<FlowFieldFollower> flowFieldFollowerEnabled, RefRW<UnitMover> unitMover) in SystemAPI
                     .Query<RefRO<LocalTransform>, RefRW<FlowFieldFollower>, EnabledRefRW<FlowFieldFollower>,
                         RefRW<UnitMover>>())
        {
            int2 gridPosition =
                GridSystem.GetGridPosition(localTransform.ValueRO.Position, gridSystemData.gridNodeSize);
            int index = GridSystem.CalculateIndex(gridPosition, gridSystemData.width);
            Entity gridNodeEntity =
                gridSystemData.gridMapArray[flowFieldFollower.ValueRO.gridIndex].gridEntityArray[index];
            GridSystem.GridNode gridNode = SystemAPI.GetComponent<GridSystem.GridNode>(gridNodeEntity);
            float3 gridNodeMoveVector = GridSystem.GetWorldMovementVector(gridNode.vector);

            if (GridSystem.IsWall(gridNode))
            {
                gridNodeMoveVector = flowFieldFollower.ValueRO.lastMoveVector;
            }
            else
            {
                flowFieldFollower.ValueRW.lastMoveVector = gridNodeMoveVector;
            }

            unitMover.ValueRW.targetPosition =
                GridSystem.GetWorldCenterPosition(gridPosition.x, gridPosition.y, gridSystemData.gridNodeSize) +
                gridNodeMoveVector * (gridSystemData.gridNodeSize * 2f);

            if (math.distance(localTransform.ValueRO.Position, flowFieldFollower.ValueRO.targetPosition) <
                gridSystemData.gridNodeSize)
            {
                unitMover.ValueRW.targetPosition = localTransform.ValueRO.Position;
                flowFieldFollowerEnabled.ValueRW = false;
            }

            RaycastInput raycastInput = new RaycastInput
            {
                Start = localTransform.ValueRO.Position,
                End = flowFieldFollower.ValueRO.targetPosition,
                Filter = new CollisionFilter
                {
                    BelongsTo = ~0u,
                    CollidesWith = 1u << GameAssets.PATHFINDING_WALLS,
                    GroupIndex = 0
                }
            };

            if (!collisionWorld.CastRay(raycastInput))
            {
                // Did not hit anything, no wall in between
                unitMover.ValueRW.targetPosition = flowFieldFollower.ValueRO.targetPosition;
                flowFieldFollowerEnabled.ValueRW = false;
            }
        }


        var unitMoverJob = new UnitMoverJob
        {
            deltaTime = SystemAPI.Time.DeltaTime
        };

        unitMoverJob.ScheduleParallel();
    }
}

[BurstCompile]
public partial struct UnitMoverJob : IJobEntity
{
    public float deltaTime;

    private void Execute(ref LocalTransform localTransform, ref UnitMover unitMover,
        ref PhysicsVelocity physicsVelocity)
    {
        float3 moveDirection = unitMover.targetPosition - localTransform.Position;

        float reachedTargetDistanceSq = UnitMoverSystem.REACHED_TARGET_POSITION_DISTANCE_SQ;
        if (math.lengthsq(moveDirection) <= reachedTargetDistanceSq)
        {
            physicsVelocity.Linear = float3.zero;
            physicsVelocity.Angular = float3.zero;
            unitMover.isMoving = false;
            return;
        }

        unitMover.isMoving = true;
        moveDirection = math.normalize(moveDirection);

        localTransform.Rotation =
            math.slerp(localTransform.Rotation, quaternion.LookRotationSafe(moveDirection, math.up()),
                deltaTime * unitMover.rotationSpeed);
        physicsVelocity.Linear = moveDirection * unitMover.moveSpeed;
        physicsVelocity.Angular = float3.zero;
    }
}

[BurstCompile]
[WithAll(typeof(TargetPositionPathQueued))]
public partial struct TargetPositionPathQueuedJob : IJobEntity
{
    [NativeDisableParallelForRestriction]
    public ComponentLookup<TargetPositionPathQueued> targetPositionPathQueuedComponentLookup;

    [NativeDisableParallelForRestriction]
    public ComponentLookup<FlowFieldPathRequest> flowFieldPathRequestComponentLookup;

    [NativeDisableParallelForRestriction] public ComponentLookup<FlowFieldFollower> flowFieldFollowerComponentLookup;
    [NativeDisableParallelForRestriction] public ComponentLookup<MoveOverride> moveOverrideComponentLookup;

    [ReadOnly] public CollisionWorld collisionWorld;
    [ReadOnly] public int width;
    [ReadOnly] public int height;
    [ReadOnly] public NativeArray<byte> costMap;
    [ReadOnly] public float gridNodeSize;

    public void Execute(in LocalTransform localTransform, ref UnitMover unitMover, Entity entity)
    {
        RaycastInput raycastInput = new RaycastInput
        {
            Start = localTransform.Position,
            End = targetPositionPathQueuedComponentLookup[entity].targetPosition,
            Filter = new CollisionFilter
            {
                BelongsTo = ~0u,
                CollidesWith = 1u << GameAssets.PATHFINDING_WALLS,
                GroupIndex = 0
            }
        };

        if (!collisionWorld.CastRay(raycastInput))
        {
            // Did not hit anything, no wall in between
            unitMover.targetPosition = targetPositionPathQueuedComponentLookup[entity].targetPosition;
            flowFieldPathRequestComponentLookup.SetComponentEnabled(entity, false);
            flowFieldFollowerComponentLookup.SetComponentEnabled(entity, false);
        }
        else
        {
            // There is a wall in between
            if (moveOverrideComponentLookup.HasComponent(entity))
            {
                moveOverrideComponentLookup.SetComponentEnabled(entity, false);
            }
            if (GridSystem.IsValidWalkableGridPosition(targetPositionPathQueuedComponentLookup[entity].targetPosition,
                    width, height, costMap, gridNodeSize))
            {
                FlowFieldPathRequest flowFieldPathRequest = flowFieldPathRequestComponentLookup[entity];
                flowFieldPathRequest.targetPosition = targetPositionPathQueuedComponentLookup[entity].targetPosition;
                flowFieldPathRequestComponentLookup[entity] = flowFieldPathRequest;
                flowFieldPathRequestComponentLookup.SetComponentEnabled(entity, true);
            }
            else
            {
                unitMover.targetPosition = localTransform.Position;
                flowFieldPathRequestComponentLookup.SetComponentEnabled(entity, false);
                flowFieldFollowerComponentLookup.SetComponentEnabled(entity, false);
            }
        }
        targetPositionPathQueuedComponentLookup.SetComponentEnabled(entity, false);
    }
}