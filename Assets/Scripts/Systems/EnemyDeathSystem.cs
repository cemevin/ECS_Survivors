using Unity.Burst;
using Unity.Entities;

[BurstCompile]
[UpdateInGroup(typeof(SimulationSystemGroup))]
public partial struct EnemyDeathSystem : ISystem
{
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<EnemyTag>();
    }

    public void OnUpdate(ref SystemState state) 
    {
        var ecbSingleton = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>();
        var ecb = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged);

        foreach (var (animator, deathData, entity) in
            SystemAPI.Query<RefRW<SpriteAnimator>, RefRO<DeathAnimData>>()
                     .WithAll<EnemyTag, DyingTag>().WithEntityAccess())
        {
            // first frame of dying - switch to death animation
            if (animator.ValueRO.IsLooping)
            {
                animator.ValueRW.CurrentRow = deathData.ValueRO.RowIndex;
                animator.ValueRW.FrameRate = deathData.ValueRO.FrameRate;
                animator.ValueRW.CurrentFrame = 0;
                animator.ValueRW.ElapsedTime = 0f;
                animator.ValueRW.IsLooping = false;
                animator.ValueRW.CompleteSince = 0;
            }

            // destroy when animation completes
            if (animator.ValueRO.CompleteSince > 5)
            {
                ecb.DestroyEntity(entity);
            }
        }
    }
}