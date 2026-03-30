using System.Diagnostics;
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;

[BurstCompile]
[UpdateInGroup(typeof(SimulationSystemGroup))]
public partial struct SpriteAnimationSystem : ISystem
{
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<EnemyTag>();
    }

    public void OnUpdate(ref SystemState state)
    {
        float dt = SystemAPI.Time.DeltaTime;

        foreach (var (animator, offset) in
            SystemAPI.Query<RefRW<SpriteAnimator>, RefRW<SpriteFrameOffset>>())
        {
            animator.ValueRW.ElapsedTime += dt;
            if (animator.ValueRO.ElapsedTime < 1f / animator.ValueRO.FrameRate) continue;

            animator.ValueRW.ElapsedTime = 0f;

            int nextFrame = animator.ValueRO.CurrentFrame + 1;

            if (nextFrame >= animator.ValueRO.TotalFrames)
            {
                if (animator.ValueRO.IsLooping)
                    animator.ValueRW.CurrentFrame = 0;
                else
                    animator.ValueRW.CompleteSince++; 
            }
            else
            {
                animator.ValueRW.CurrentFrame = nextFrame;
            }


            int col = animator.ValueRO.CurrentFrame % animator.ValueRO.Columns;
            int row = animator.ValueRO.CurrentRow;

            offset.ValueRW.Value = new float2(
                (float)col / animator.ValueRO.Columns,
                (float)row / animator.ValueRO.Rows
            );
        }
    }
}