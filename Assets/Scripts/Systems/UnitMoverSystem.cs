using Unity.Burst;
using Unity.Entities;
using Unity.Transforms;
using Unity.Mathematics;
using Unity.Physics;

partial struct UnitMoverSystem : ISystem
{
    public const float REACHED_TARGET_POSITION_DISTANCE_SQ = 2f;

    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<GridSystem.GridSystemData>();
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        GridSystem.GridSystemData gridSystemData = SystemAPI.GetSingleton<GridSystem.GridSystemData>();
        foreach ((RefRO<LocalTransform> localTransform, RefRW<FlowFieldFollower> flowFieldFollower,
                     EnabledRefRW<FlowFieldFollower> flowFieldFollowerEnabled, RefRW<UnitMover> unitMover) in SystemAPI
                     .Query<RefRO<LocalTransform>, RefRW<FlowFieldFollower>, EnabledRefRW<FlowFieldFollower>,
                         RefRW<UnitMover>>())
        {
            int2 gridPosition =
                GridSystem.GetGridPosition(localTransform.ValueRO.Position, gridSystemData.gridNodeSize);
            int index = GridSystem.CalculateIndex(gridPosition, gridSystemData.width);
            Entity gridNodeEntity = gridSystemData.gridMap.gridEntityArray[index];
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

            if (math.distance(localTransform.ValueRO.Position, flowFieldFollower.ValueRO.targetPosition) < gridSystemData.gridNodeSize)
            {
                unitMover.ValueRW.targetPosition = localTransform.ValueRO.Position;
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