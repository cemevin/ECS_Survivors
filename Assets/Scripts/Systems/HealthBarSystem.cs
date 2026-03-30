using Unity.Burst;
using Unity.Entities;
using Unity.Transforms;

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

        foreach (var (owner, fill, entity) in
            SystemAPI.Query<RefRO<HealthBarOwner>, RefRW<HealthBarFill>>().WithEntityAccess())
        {
            /* if (SystemAPI.IsComponentEnabled<DyingTag>(owner.ValueRO.EnemyEntity)) 
            {
                ecb.DestroyEntity(entity);
                continue;
            } */
            if (SystemAPI.IsComponentEnabled<HealthDirty>(owner.ValueRO.EnemyEntity))
            {
                var health = SystemAPI.GetComponent<Health>(owner.ValueRO.EnemyEntity);
                fill.ValueRW.Value = health.Current / health.Max;   
                SystemAPI.SetComponentEnabled<HealthDirty>(owner.ValueRO.EnemyEntity, false);
            }
        }
    }
}