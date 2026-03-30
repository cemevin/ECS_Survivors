using Unity.Burst;
using Unity.Entities;
using Unity.Transforms;
using Unity.Collections;
using Unity.Mathematics;

[BurstCompile]
public partial struct SpawnSystem : ISystem
{
    public void OnUpdate(ref SystemState state)
    {
        float dt = SystemAPI.Time.DeltaTime;
        var ecbSingleton = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>();
        var ecb = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged);

        // Get player position — query for the singleton
        var playerQuery = SystemAPI.QueryBuilder()
            .WithAll<PlayerTag, LocalTransform>().Build();
        if (playerQuery.IsEmpty) return;

        float3 playerPos = playerQuery.GetSingleton<LocalTransform>().Position;

        foreach (var spawner in
            SystemAPI.Query<RefRW<SpawnerData>>())
        {
            spawner.ValueRW.Timer -= dt;
            if (spawner.ValueRO.Timer > 0) continue;
            
            float newInterval = math.max(
                spawner.ValueRO.MinInterval,
                spawner.ValueRO.BaseInterval - spawner.ValueRO.RampRate
            );
            spawner.ValueRW.BaseInterval = newInterval;
            spawner.ValueRW.Timer = spawner.ValueRO.BaseInterval;

            int numEnemies = spawner.ValueRW.Rng.NextInt(spawner.ValueRO.MinSpawnCount, spawner.ValueRO.MaxSpawnCount);
            for (int i = 0; i < numEnemies; i++)
            {
                // random point in band around player
                float angle = spawner.ValueRW.Rng.NextFloat(0, math.PI2);
                float radius = spawner.ValueRW.Rng.NextFloat(
                    spawner.ValueRO.SpawnRadius,
                    spawner.ValueRO.SpawnRadius + spawner.ValueRO.SpawnBandWidth);

                float3 spawnPos = playerPos + new float3(
                    math.cos(angle) * radius,
                    0,
                    math.sin(angle) * radius);

                Entity e = ecb.Instantiate(spawner.ValueRO.EnemyPrefab);
                ecb.SetComponent(e, LocalTransform.FromPositionRotationScale(
                    spawnPos,
                    quaternion.Euler(math.radians(90f), 0f, 0f),
                    1f
                ));
            }
        }
    }
}