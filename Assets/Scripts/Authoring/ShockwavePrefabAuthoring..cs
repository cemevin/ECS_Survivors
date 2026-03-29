using Unity.Entities;
using Unity.Transforms;
using UnityEngine;
public class ShockwavePrefabAuthoring : MonoBehaviour
{
    public GameObject Prefab;

    class Baker : Baker<ShockwavePrefabAuthoring>
    {
        public override void Bake(ShockwavePrefabAuthoring authoring)
        {
            var e = GetEntity(TransformUsageFlags.None); // None, not Dynamic - this is just a holder
            AddComponent(e, new ShockwavePrefabRef
            { 
                Value = GetEntity(authoring.Prefab, TransformUsageFlags.Dynamic)
            });
        }
    }
}