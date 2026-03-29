using Unity.Entities;
using Unity.Transforms;
using UnityEngine;

public class ShockwaveAuthoring : MonoBehaviour
{
    public float ExpandSpeed = 1f;
    public float Lifetime = 5f;
    public float Damage = 10f; 
    public float Radius = 100f;
    public float PushForce = 0.1f;

    class Baker : Baker<ShockwaveAuthoring>
    {
        public override void Bake(ShockwaveAuthoring authoring)
        {
            var e = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(e, new ShockwaveTag());
            AddComponent(e, new ShockwaveData {MaxRadius = authoring.Radius, CurrentRadius = 0, PushForce = authoring.PushForce, ExpandSpeed = authoring.ExpandSpeed });
            AddComponent(e, new PostTransformMatrix());
        }
    }
}