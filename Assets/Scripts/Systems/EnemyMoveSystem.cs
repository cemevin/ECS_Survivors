using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

[BurstCompile]
public partial struct EnemyMoveSystem : ISystem
{
    public void OnUpdate(ref SystemState state)
    {
        // Get player position — query for the singleton
        var playerQuery = SystemAPI.QueryBuilder()
            .WithAll<PlayerTag, LocalTransform, Health>().Build();
        if (playerQuery.IsEmpty) return;

        float3 playerPos = playerQuery.GetSingleton<LocalTransform>().Position;

        float dt = SystemAPI.Time.DeltaTime;

        foreach (var (transform, impulseVelocity, damage, speed) in
            SystemAPI.Query<RefRW<LocalTransform>, RefRW<ImpulseVelocity>, RefRO<Damage>, RefRO<MoveSpeed>>()
                     .WithAll<EnemyTag>())
        {
            // combine player seek + impulse velocities and update transform
            float3 distvec = playerPos - transform.ValueRO.Position;
            float dist = math.length(distvec);
            if (dist < 1f)
            {
                impulseVelocity.ValueRW.Value = 0;
                continue;
            }

            float3 dir = distvec/dist;
            float3 seekVel = dir * speed.ValueRO.Value;
            float3 total = seekVel + impulseVelocity.ValueRO.Value;
            transform.ValueRW.Position += total * dt;

            // decay impulse
            impulseVelocity.ValueRW.Value *= math.exp(-8f * dt); // tune the 8f for bleed-off speed
        }
    }
}