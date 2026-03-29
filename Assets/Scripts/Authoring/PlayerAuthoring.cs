using Unity.Entities;
using UnityEngine;
using Unity.Mathematics;

public class PlayerAuthoring : MonoBehaviour
{
    public float MoveSpeed = 3f;
    public float MaxHealth = 30f;
    public float FireRate = 0.25f;
    public int ProjectileCount = 8;

    class Baker : Baker<PlayerAuthoring>
    {
        public override void Bake(PlayerAuthoring authoring)
        {
            var e = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(e, new PlayerTag());
            AddComponent(e, new MoveSpeed { Value = authoring.MoveSpeed });
            AddComponent(e, new Health { Current = authoring.MaxHealth, Max = authoring.MaxHealth });
            AddComponent(e, new PlayerInput());
            AddComponent(e, new AimDirection { Value = new float3(0, 0, 1) });

            AddComponent(e, new WeaponData 
            { 
                FireRate = authoring.FireRate, 
                Timer = 0f,
                ProjectileCount = authoring.ProjectileCount 
            });
        }
    }
}