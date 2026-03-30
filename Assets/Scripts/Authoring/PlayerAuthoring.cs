using Unity.Entities;
using UnityEngine;
using Unity.Mathematics;

public class PlayerAuthoring : MonoBehaviour
{
    public float MoveSpeed = 3f;
    public float MaxHealth = 30f;
    public float FireRate = 0.25f;
    public int ProjectileCount = 8;
    public float ShockwaveCooldownDuration = 10;

    class Baker : Baker<PlayerAuthoring>
    {
        public override void Bake(PlayerAuthoring authoring)
        {
            var e = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(e, new PlayerTag());
            AddComponent(e, new MoveSpeed { Value = authoring.MoveSpeed });
            AddComponent(e, new Health { Current = authoring.MaxHealth, Max = authoring.MaxHealth });
            AddComponent(e, new PlayerAimDirection()); 

            AddComponent(e, new WeaponData 
            { 
                FireRate = authoring.FireRate, 
                Timer = 0f,
                ProjectileCount = authoring.ProjectileCount ,
                ShockwaveCooldownDuration = authoring.ShockwaveCooldownDuration
            });
        }
    }
}