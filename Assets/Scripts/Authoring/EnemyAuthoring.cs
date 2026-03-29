using Unity.Entities;
using UnityEngine;

public class EnemyAuthoring : MonoBehaviour
{
    public float MoveSpeed = 3f;
    public float MaxHealth = 30f;
    public float DPSOnContact = 1f;

    class Baker : Baker<EnemyAuthoring>
    {
        public override void Bake(EnemyAuthoring authoring)
        {
            var e = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(e, new EnemyTag());
            AddComponent(e, new MoveSpeed { Value = authoring.MoveSpeed });
            AddComponent(e, new Health { Current = authoring.MaxHealth, Max = authoring.MaxHealth });
            AddComponent(e, new Damage { Value = authoring.DPSOnContact });
            AddComponent(e, new Direction());
            AddComponent(e, new ImpulseVelocity());
            AddComponent(e, new ShockwaveImpactCooldown());
            SetComponentEnabled<ShockwaveImpactCooldown>(e, false);
        }
    }
}