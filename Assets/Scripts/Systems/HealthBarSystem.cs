using Unity.Burst;
using Unity.Entities;
using Unity.Transforms;
using Unity.Mathematics;

[BurstCompile]
[UpdateInGroup(typeof(SimulationSystemGroup))]
public partial struct HealthBarSystem : ISystem
{
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<EnemyTag>();
    }

    public void OnUpdate(ref SystemState state)
    {
        var ecbSingleton = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>();
        var ecb = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged);

        foreach (var (owner, fill, transform, entity) in
            SystemAPI.Query<RefRO<HealthBarOwner>, RefRW<HealthBarFill>, RefRW<LocalTransform>>().WithEntityAccess())
        {
            if (owner.ValueRO.EnemyEntity == Entity.Null || !SystemAPI.HasComponent<Health>(owner.ValueRO.EnemyEntity))
            {
                ecb.DestroyEntity(entity);
                continue;
            }
            if (!SystemAPI.HasComponent<LocalTransform>(owner.ValueRO.EnemyEntity)) continue;

            var health = SystemAPI.GetComponent<Health>(owner.ValueRO.EnemyEntity);
            fill.ValueRW.Value = health.Current / health.Max;

            // manually follow enemy position
            var enemyTransform = SystemAPI.GetComponent<LocalTransform>(owner.ValueRO.EnemyEntity);
            transform.ValueRW.Position = enemyTransform.Position + new float3(0, 1.5f, 0);
        }
    }
}