using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

[BurstCompile]
public partial struct ShockwaveSystem : ISystem
{
    public void OnUpdate(ref SystemState state)
    {
        float dt = SystemAPI.Time.DeltaTime;
        var ecb = new EntityCommandBuffer(Allocator.Temp);

        // Expand all active shockwaves
        foreach (var (shockwave, transform, postTransform,entity) in
            SystemAPI.Query<RefRW<ShockwaveData>, RefRO<LocalTransform>, RefRW<PostTransformMatrix>>()
                     .WithAll<ShockwaveTag>().WithEntityAccess())
        {
            shockwave.ValueRW.CurrentRadius += shockwave.ValueRO.ExpandSpeed * dt;

            if (shockwave.ValueRO.CurrentRadius >= shockwave.ValueRO.MaxRadius)
            {
                ecb.DestroyEntity(entity);
                continue;
            }

            postTransform.ValueRW.Value = float4x4.Scale(shockwave.ValueRO.CurrentRadius, 1f, shockwave.ValueRO.CurrentRadius);

            float3 origin = transform.ValueRO.Position;
            float r = shockwave.ValueRO.CurrentRadius;
            float force = shockwave.ValueRO.PushForce;

            // Push enemies within the ring (thin band check)
            foreach (var (enemyTransform, impulseVelocity) in
                SystemAPI.Query<RefRO<LocalTransform>, RefRW<ImpulseVelocity>>().WithAll<EnemyTag>())
            {
                float dist = math.distance(origin, enemyTransform.ValueRO.Position);
                if (dist < r + 1f && dist > r - 1f) // 2-unit wide ring
                {
                    float3 pushDir = math.normalizesafe(enemyTransform.ValueRO.Position - origin);
                    impulseVelocity.ValueRW.Value += pushDir * force;
                }
            }
        }

        ecb.Playback(state.EntityManager);
        ecb.Dispose();
    }
}