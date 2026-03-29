using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

[BurstCompile]
[UpdateInGroup(typeof(SimulationSystemGroup))]
public partial struct ShockwaveSpawnSystem : ISystem
{
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<PlayerTag>();
        state.RequireForUpdate<ShockwavePrefabRef>();
    }

    public void OnUpdate(ref SystemState state)
    {
        var ecbSingleton = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>();
        var ecb = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged);
        Entity prefab = SystemAPI.GetSingleton<ShockwavePrefabRef>().Value;
        
        float elapsed = (float)SystemAPI.Time.ElapsedTime;

        foreach (var (input, weapon, transform) in
            SystemAPI.Query<RefRO<PlayerInput>, RefRW<WeaponData>, RefRO<LocalTransform>>()
                     .WithAll<PlayerTag>())
        {
            if (!input.ValueRO.ShockwavePressed || weapon.ValueRO.CurrentShockwaveCooldown > elapsed) return;

            weapon.ValueRW.CurrentShockwaveCooldown = elapsed + weapon.ValueRO.ShockwaveCooldownDuration;

            Entity e = ecb.Instantiate(prefab);
            ecb.SetComponent(e, LocalTransform.FromPositionRotationScale(
                transform.ValueRO.Position, 
                quaternion.Euler(math.radians(90f), 0f, 0f), 
                100f)
            );
        }
    }
}