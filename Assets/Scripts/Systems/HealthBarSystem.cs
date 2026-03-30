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
            if (SystemAPI.HasComponent<DyingTag>(owner.ValueRO.EnemyEntity))
            {
                ecb.DestroyEntity(entity);
                continue;
            }

            var health = SystemAPI.GetComponent<Health>(owner.ValueRO.EnemyEntity);
            fill.ValueRW.Value = health.Current / health.Max;
        }
    }
}