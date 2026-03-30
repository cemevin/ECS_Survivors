using Unity.Entities;
using UnityEngine;

[System.Serializable]
public struct SpriteInfo
{
    public int TotalFrames;
    public int Columns;
    public int Rows;
    public float FrameRate;

    public SpriteInfo(int InTotalFrames, int InColumns, int InRows, float InFrameRate)
    {
        TotalFrames = InTotalFrames;
        Columns = InColumns;
        Rows = InRows;
        FrameRate = InFrameRate;
    }
}
public class EnemyAuthoring : MonoBehaviour
{
    public float MoveSpeed = 3f;
    public float MaxHealth = 30f;
    public float DPSOnContact = 1f;
    public SpriteInfo Sprite = new SpriteInfo(6, 6, 2, 10f);
    public int DeathAnimRowIndex = 1;
    public float DeathAnimFrameRate = 2;

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

            AddComponent(e, new SpriteAnimator
            {
                TotalFrames = authoring.Sprite.TotalFrames,
                Columns = authoring.Sprite.Columns,
                Rows = authoring.Sprite.Rows,
                FrameRate = authoring.Sprite.FrameRate,
                CurrentFrame = 0,
                ElapsedTime = 0f,
                IsLooping = true
            });

            AddComponent(e, new SpriteFrameOffset());
            AddComponent(e, new SpriteFlipX { Value = 0f });

            AddComponent(e, new DeathAnimData
            {
                RowIndex = authoring.DeathAnimRowIndex,
                FrameRate = authoring.DeathAnimFrameRate
            });

            AddComponent(e, new DyingTag());
            SetComponentEnabled<DyingTag>(e, false); 
            
            AddComponent(e, new HealthDirty());
            SetComponentEnabled<HealthDirty>(e, false);
        }
    }
}