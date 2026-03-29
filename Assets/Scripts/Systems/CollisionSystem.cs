using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Collections;

[UpdateInGroup(typeof(SimulationSystemGroup))]
[BurstCompile]
public partial struct CollisionSystem : ISystem
{
    public void OnUpdate(ref SystemState state)
    {
        var ecbSingleton = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>();
        var ecb = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged);

        foreach (var (projTransform, damage, impulseOnHit, projEntity) in
            SystemAPI.Query<RefRO<LocalTransform>, RefRO<Damage>, RefRO<ImpulseOnHit>>()
                     .WithAll<ProjectileTag>().WithEntityAccess())
        {
            foreach (var (enemyTransform, health, impulseVelocity, enemyEntity) in
                SystemAPI.Query<RefRO<LocalTransform>, RefRW<Health>, RefRW<ImpulseVelocity>>()
                         .WithAll<EnemyTag>().WithEntityAccess())
            {
                float dist = math.distance(
                    projTransform.ValueRO.Position,
                    enemyTransform.ValueRO.Position);

                if (dist > 0.8f) continue;

                health.ValueRW.Current -= damage.ValueRO.Value;

                ecb.DestroyEntity(projEntity);

                if (health.ValueRO.Current <= 0f)
                    ecb.DestroyEntity(enemyEntity);
                else
                {
                    var impulseDir = math.normalizesafe(enemyTransform.ValueRO.Position - projTransform.ValueRO.Position);
                    impulseVelocity.ValueRW.Value += impulseDir * impulseOnHit.ValueRO.Value;
                }

                break; // projectile is gone, stop checking enemies
            }
        }
    }
}