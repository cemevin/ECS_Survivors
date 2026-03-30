using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

/* [UpdateInGroup(typeof(SimulationSystemGroup))]
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
                         .WithAll<EnemyTag>().WithDisabled<DyingTag>() 
                         .WithEntityAccess())
            {
                float dist = math.distance(
                    projTransform.ValueRO.Position,
                    enemyTransform.ValueRO.Position);

                if (dist > 0.8f) continue;

                health.ValueRW.Current -= damage.ValueRO.Value;

                ecb.DestroyEntity(projEntity);

                if (health.ValueRO.Current <= 0f)
                {
                    ecb.SetComponentEnabled<DyingTag>(enemyEntity, true);
                }
                else
                {
                    var impulseDir = math.normalizesafe(enemyTransform.ValueRO.Position - projTransform.ValueRO.Position);
                    impulseVelocity.ValueRW.Value += impulseDir * impulseOnHit.ValueRO.Value;
                }

                break; // projectile is gone, stop checking enemies
            }
        }
    }
} */

[BurstCompile]
[UpdateInGroup(typeof(SimulationSystemGroup))]
public partial struct CollisionSystem : ISystem
{
    public void OnUpdate(ref SystemState state)
    {
        var ecbSingleton = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>();
        var ecb = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged);

        // count enemies for hashmap capacity
        var enemyQuery = SystemAPI.QueryBuilder()
            .WithAll<EnemyTag, LocalTransform>()
            .WithDisabled<DyingTag>().Build();
        int enemyCount = enemyQuery.CalculateEntityCount();
        if (enemyCount == 0) return;

        const float collisionRadius = 0.8f;

        const float cellSize = collisionRadius * 2;

        // bucket enemies by cell
        var grid = new NativeParallelMultiHashMap<int2, Entity>(enemyCount, Allocator.Temp);

        foreach (var (transform, entity) in
            SystemAPI.Query<RefRO<LocalTransform>>()
                     .WithAll<EnemyTag>().WithDisabled<DyingTag>()
                     .WithEntityAccess())
        {
            int2 cell = (int2)math.floor(transform.ValueRO.Position.xz / cellSize);
            grid.Add(cell, entity);
        }

        // check projectiles only against their cell
        foreach (var (projTransform, damage, impulseOnHit, projEntity) in
            SystemAPI.Query<RefRO<LocalTransform>, RefRO<Damage>, RefRO<ImpulseOnHit>>()
                     .WithAll<ProjectileTag>().WithEntityAccess())
        {
            int2 cell = (int2)math.floor(projTransform.ValueRO.Position.xz / cellSize);
            if (!grid.TryGetFirstValue(cell, out Entity enemyEntity, out var it))
                continue;

            do
            {
                var enemyTransform = SystemAPI.GetComponent<LocalTransform>(enemyEntity);
                float dist = math.distance(
                    projTransform.ValueRO.Position,
                    enemyTransform.Position);

                if (dist > collisionRadius) continue;

                var health = SystemAPI.GetComponentRW<Health>(enemyEntity);
                health.ValueRW.Current -= damage.ValueRO.Value;
                ecb.SetComponentEnabled<HealthDirty>(enemyEntity, true);
                ecb.DestroyEntity(projEntity); 

                if (health.ValueRO.Current <= 0f)
                    ecb.SetComponentEnabled<DyingTag>(enemyEntity, true);
                else
                {
                    var impulse = SystemAPI.GetComponentRW<ImpulseVelocity>(enemyEntity);
                    var impulseDir = math.normalizesafe(enemyTransform.Position - projTransform.ValueRO.Position);
                    impulse.ValueRW.Value += impulseDir * impulseOnHit.ValueRO.Value;
                }
                break;
            }
            while (grid.TryGetNextValue(out enemyEntity, ref it));
        }

        grid.Dispose(); 
    }
}