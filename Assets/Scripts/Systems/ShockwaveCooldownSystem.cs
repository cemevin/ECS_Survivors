using Unity.Burst;
using Unity.Entities;


[BurstCompile]
[UpdateInGroup(typeof(SimulationSystemGroup))]
public partial struct ShockwaveCooldownSystem : ISystem
{
    public void OnUpdate(ref SystemState state)
    {
        float elapsed = (float)SystemAPI.Time.ElapsedTime;

        foreach (var (cooldown, entity) in
            SystemAPI.Query<RefRO<ShockwaveImpactCooldown>>()
                     .WithAll<EnemyTag>()
                     .WithEntityAccess())
        {
            if (elapsed >= cooldown.ValueRO.ExpiryTime)
                SystemAPI.SetComponentEnabled<ShockwaveImpactCooldown>(entity, false);
        }
    }
}