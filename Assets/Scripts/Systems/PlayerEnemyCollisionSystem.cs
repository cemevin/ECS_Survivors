using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

[BurstCompile]
[UpdateInGroup(typeof(SimulationSystemGroup))]
[UpdateAfter(typeof(CollisionSystem))]
public partial struct PlayerEnemyCollisionSystem : ISystem
{
    public void OnUpdate(ref SystemState state)
    {
        foreach (var (playerTransform, health) in
            SystemAPI.Query<RefRO<LocalTransform>, RefRW<Health>>()
                     .WithAll<PlayerTag>())
        {
            foreach (var (enemyTransform, damage) in
                SystemAPI.Query<RefRO<LocalTransform>, RefRO<Damage>>()
                         .WithAll<EnemyTag>())
            {
                float dist = math.distance(
                    playerTransform.ValueRO.Position,
                    enemyTransform.ValueRO.Position);

                if (dist > 1f) continue;

                health.ValueRW.Current -= damage.ValueRO.Value * SystemAPI.Time.DeltaTime;
            }
        }
    }
}