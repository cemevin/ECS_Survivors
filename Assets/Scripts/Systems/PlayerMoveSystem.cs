using Unity.Burst;
using Unity.Entities;
using Unity.Transforms;

[BurstCompile]
public partial struct PlayerMoveSystem : ISystem
{
    public void OnUpdate(ref SystemState state)
    {
        float dt = SystemAPI.Time.DeltaTime;

        foreach (var (transform, input, speed) in
            SystemAPI.Query<RefRW<LocalTransform>, RefRO<PlayerInput>, RefRO<MoveSpeed>>()
                     .WithAll<PlayerTag>())
        {
            transform.ValueRW.Position += input.ValueRO.MoveDirection * speed.ValueRO.Value * dt;
        }
    }
}