using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

[BurstCompile]
public partial struct ShockwaveSystem : ISystem
{
    public void OnUpdate(ref SystemState state)
    {
        float dt = SystemAPI.Time.DeltaTime;
        var ecbSingleton = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>();
        var ecb = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged);

        // collect active shockwave data first
        var shockwaves = new NativeList<(float dmg, float3 origin, float radius, float force)>(Allocator.Temp);

        foreach (var (shockwave, shockwaveRadius, transform, entity) in
            SystemAPI.Query<RefRW<ShockwaveData>, RefRW<ShockwaveRadius>, RefRW<LocalTransform>>()
                     .WithAll<ShockwaveTag>().WithEntityAccess())
        {
            shockwave.ValueRW.CurrentRadius += shockwave.ValueRO.ExpandSpeed * dt;

            if (shockwave.ValueRO.CurrentRadius >= shockwave.ValueRO.MaxRadius)
            {
                ecb.DestroyEntity(entity);
                continue;
            }

            float quadWorldSize = 100f; // should match quad's actual world size
            shockwaveRadius.ValueRW.Value = shockwave.ValueRO.CurrentRadius / quadWorldSize;

            shockwaves.Add((shockwave.ValueRO.Damage, transform.ValueRO.Position, 
                           shockwave.ValueRO.CurrentRadius, 
                           shockwave.ValueRO.PushForce));
        }

        if (shockwaves.Length>0)
        {
            // then iterate enemies against collected shockwave data
            foreach (var (enemyTransform, health, impulse, enemyEntity) in
                SystemAPI.Query<RefRO<LocalTransform>, RefRW<Health>, RefRW<ImpulseVelocity>>()
                        .WithAll<EnemyTag>().WithAbsent<DyingTag>().WithDisabled<ShockwaveImpactCooldown>()
                        .WithEntityAccess())
            {
                for (int i = 0; i < shockwaves.Length; i++)
                {
                    var (dmg, origin, r, force) = shockwaves[i];
                    float dist = math.distance(origin, enemyTransform.ValueRO.Position);
                    if (dist < r + 0.5f && dist > r - 0.5f)
                    {
                        health.ValueRW.Current -= dmg;

                        if (health.ValueRO.Current <= 0f)
                        {
                            ecb.AddComponent(enemyEntity, new DyingTag());
                        }
                        else
                        {
                            float3 pushDir = math.normalizesafe(enemyTransform.ValueRO.Position - origin);
                            impulse.ValueRW.Value += pushDir * force;
                            ecb.SetComponentEnabled<ShockwaveImpactCooldown>(enemyEntity, true);
                            ecb.SetComponent(enemyEntity, new ShockwaveImpactCooldown 
                            { 
                                ExpiryTime = (float)SystemAPI.Time.ElapsedTime + 0.5f 
                            });
                        }
                    }
                }
            }
        }

        shockwaves.Dispose();
    }
}