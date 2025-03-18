using Unity.Burst;
using Unity.Entities;
using Unity.Rendering;

[UpdateBefore(typeof(ActiveAnimationSystem))]
partial struct ChangeAnimationSystem : ISystem
{
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<AnimationDataHolder>();
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        AnimationDataHolder animationDataHolder = SystemAPI.GetSingleton<AnimationDataHolder>();

        ChangeAnimationJob changeAnimationJob = new ChangeAnimationJob
        {
            animationDataBlobArrayBlobAssetReference = animationDataHolder.animationDataBlobArrayBlobAssetReference
        };
        changeAnimationJob.ScheduleParallel();

        // foreach ((RefRW<ActiveAnimation> activeAnimation, RefRW<MaterialMeshInfo> materialMeshInfo) in SystemAPI
        //              .Query<RefRW<ActiveAnimation>, RefRW<MaterialMeshInfo>>())
        // {
        //     if (activeAnimation.ValueRO.activeAnimationType == AnimationDataSO.AnimationType.SoldierShoot)
        //     {
        //         continue;
        //     }
        //     if (activeAnimation.ValueRO.activeAnimationType == AnimationDataSO.AnimationType.ZombieAttack)
        //     {
        //         continue;
        //     }
        //
        //     if (activeAnimation.ValueRO.activeAnimationType != activeAnimation.ValueRO.nextAnimationType)
        //     {
        //         activeAnimation.ValueRW.frame = 0;
        //         activeAnimation.ValueRW.frameTimer = 0;
        //         activeAnimation.ValueRW.activeAnimationType = activeAnimation.ValueRO.nextAnimationType;
        //
        //         ref AnimationData animationData =
        //             ref animationDataHolder.animationDataBlobArrayBlobAssetReference.Value[
        //                 (int)activeAnimation.ValueRW.activeAnimationType];
        //         materialMeshInfo.ValueRW.MeshID = animationData.batchMeshIdBlobArray[0];
        //     }
        // }
    }
}

[BurstCompile]
public partial struct ChangeAnimationJob : IJobEntity
{
    public BlobAssetReference<BlobArray<AnimationData>> animationDataBlobArrayBlobAssetReference;

    public void Execute(ref ActiveAnimation activeAnimation, ref MaterialMeshInfo materialMeshInfo)
    {
        if (activeAnimation.activeAnimationType == AnimationDataSO.AnimationType.SoldierShoot)
        {
            return;
        }
        if (activeAnimation.activeAnimationType == AnimationDataSO.AnimationType.ZombieAttack)
        {
            return;
        }

        if (activeAnimation.activeAnimationType != activeAnimation.nextAnimationType)
        {
            activeAnimation.frame = 0;
            activeAnimation.frameTimer = 0;
            activeAnimation.activeAnimationType = activeAnimation.nextAnimationType;

            ref AnimationData animationData =
                ref animationDataBlobArrayBlobAssetReference.Value[
                    (int)activeAnimation.activeAnimationType];
            materialMeshInfo.MeshID = animationData.batchMeshIdBlobArray[0];
        }
    }
}