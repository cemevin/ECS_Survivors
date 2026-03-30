using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

[UpdateInGroup(typeof(SimulationSystemGroup))]
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
            float3 Move = new float3(input.ValueRO.MoveStick.x, 0, input.ValueRO.MoveStick.y);
            transform.ValueRW.Position += Move * speed.ValueRO.Value * dt; 
        }
    }
}