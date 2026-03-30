using Unity.Burst;
using Unity.Entities;
using Unity.Transforms;
using Unity.Mathematics;
using UnityEngine;


[BurstCompile]
[UpdateInGroup(typeof(SimulationSystemGroup))]
public partial struct WeaponSystem : ISystem
{
    public void OnUpdate(ref SystemState state)
    {
        float dt = SystemAPI.Time.DeltaTime;
        var ecbSingleton = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>();
        var ecb = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged);

        foreach (var (transform, playerInput, aimDirection, weaponData) in
            SystemAPI.Query<RefRO<LocalTransform>, RefRW<PlayerInput>, RefRW<PlayerAimDirection>, RefRW<WeaponData>>()
                    .WithAll<PlayerTag>())
        {
            weaponData.ValueRW.Timer -= dt; // always tick

            if (!playerInput.ValueRO.FireHeld) continue;
            if (weaponData.ValueRO.Timer > 0) continue;

            weaponData.ValueRW.Timer = weaponData.ValueRO.FireRate;

            float3 playerPos = transform.ValueRO.Position;

            float3 aimDir = new float3(0,0,1);
            if (playerInput.ValueRO.AimWithMouse)
            {
                Ray ray = Camera.main.ScreenPointToRay(new Vector3(playerInput.ValueRO.AimStick.x, playerInput.ValueRO.AimStick.y, 0));
                Plane groundPlane = new Plane(Vector3.up, Vector3.zero);

                if (groundPlane.Raycast(ray, out float distance))
                {
                    float3 worldPoint = (float3)ray.GetPoint(distance);
                    aimDir = math.normalizesafe(worldPoint - transform.ValueRO.Position);
                }
            }
            else
            {
                aimDir = new float3(playerInput.ValueRO.AimStick.x, 0, playerInput.ValueRO.AimStick.y);
            }
            
            // fallback if player is standing still
            if (math.lengthsq(aimDir) < 0.001f)
                aimDir = new float3(0, 0, 1);

            aimDirection.ValueRW.Value = aimDir;

            Entity projectilePrefab = SystemAPI.GetSingleton<ProjectilePrefabRef>().Value;

            float spreadAngle = math.radians(45f); // total cone width, tune this
            int count = weaponData.ValueRO.ProjectileCount;

            for (int i = 0; i < count; i++)
            { 
                // spread evenly from -spread to +spread
                float t = count == 1 ? 0 : (float)i / (count - 1); // 0..1
                float angle = math.lerp(-spreadAngle, spreadAngle, t); 

                // rotate aimDir direction by angle around Y axis
                float3 dir = MathUtils.RotateAroundY(aimDir, angle);

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
    }
}