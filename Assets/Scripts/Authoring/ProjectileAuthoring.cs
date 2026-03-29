using Unity.Entities;
using UnityEngine;

public class ProjectileAuthoring : MonoBehaviour
{
    public float MoveSpeed = 10f;
    public float Lifetime = 5f;
    public float Damage = 10f;
    public float ImpulseOnHit = 1f;
    class Baker : Baker<ProjectileAuthoring>
    {
        public override void Bake(ProjectileAuthoring authoring)
        {
            var e = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(e, new ProjectileTag());
            AddComponent(e, new MoveSpeed { Value = authoring.MoveSpeed });
            AddComponent(e, new Lifetime { Remaining = authoring.Lifetime, Total = authoring.Lifetime });
            AddComponent(e, new Direction());
            AddComponent(e, new Damage{ Value = authoring.Damage });
            AddComponent(e, new ImpulseOnHit{ Value = authoring.ImpulseOnHit });
            AddComponent(e, new PrefabScale { Value = authoring.transform.localScale.x });
        }
    }
}