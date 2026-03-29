using Unity.Burst;
using Unity.Entities;
using Unity.Transforms;
using Unity.Collections;
using Unity.Mathematics;


[BurstCompile]
public partial struct WeaponSystem : ISystem
{
    public void OnUpdate(ref SystemState state)
    {
        float dt = SystemAPI.Time.DeltaTime;
        var ecb = new EntityCommandBuffer(Allocator.Temp);

        foreach (var (transform, playerInput, aimDir, weaponData) in
            SystemAPI.Query<RefRO<LocalTransform>, RefRO<PlayerInput>, RefRO<AimDirection>, RefRW<WeaponData>>()
                    .WithAll<PlayerTag>())
        {
            weaponData.ValueRW.Timer -= dt; // always tick

            if (!playerInput.ValueRO.FirePressed) continue;
            if (weaponData.ValueRO.Timer > 0) continue;

            weaponData.ValueRW.Timer = weaponData.ValueRO.FireRate;

            float3 playerPos = transform.ValueRO.Position;
            float3 forward = aimDir.ValueRO.Value;
            Entity projectilePrefab = SystemAPI.GetSingleton<ProjectilePrefabRef>().Value;
            
            // fallback if player is standing still
            if (math.lengthsq(forward) < 0.001f)
                forward = new float3(0, 0, 1);

            float spreadAngle = math.radians(45f); // total cone width, tune this
            int count = weaponData.ValueRO.ProjectileCount;

            for (int i = 0; i < count; i++)
            { 
                // spread evenly from -spread to +spread
                float t = count == 1 ? 0 : (float)i / (count - 1); // 0..1
                float angle = math.lerp(-spreadAngle, spreadAngle, t); 

                // rotate forward direction by angle around Y axis
                float3 dir = MathUtils.RotateAroundY(forward, angle);

                Entity spawned = ecb.Instantiate(projectilePrefab);
                ecb.SetComponent(spawned, LocalTransform.FromPosition(playerPos));

                float scale = SystemAPI.GetComponent<PrefabScale>(projectilePrefab).Value;
                ecb.SetComponent(spawned, LocalTransform.FromPositionRotationScale(
                    playerPos,
                    quaternion.identity,
                    scale
                ));

                ecb.SetComponent(spawned, new Direction { Value = dir });
            }
        }

        ecb.Playback(state.EntityManager);
        ecb.Dispose();
    }
}