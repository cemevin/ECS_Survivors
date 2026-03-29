using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

[BurstCompile]
public partial struct MoveSystem : ISystem
{
    public void OnUpdate(ref SystemState state)
    {
        float dt = SystemAPI.Time.DeltaTime;

        foreach (var (transform, direction, speed) in
            SystemAPI.Query<RefRW<LocalTransform>, RefRO<Direction>, RefRO<MoveSpeed>>())
        {
            transform.ValueRW.Position += direction.ValueRO.Value * dt * speed.ValueRO.Value;
        }
    }
}