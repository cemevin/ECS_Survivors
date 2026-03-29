using Unity.Entities;
using Unity.Transforms;
using UnityEngine;
public class ProjectilePrefabAuthoring : MonoBehaviour
{
    public GameObject Prefab;

    class Baker : Baker<ProjectilePrefabAuthoring>
    {
        public override void Bake(ProjectilePrefabAuthoring authoring)
        {
            var e = GetEntity(TransformUsageFlags.None); // None, not Dynamic - this is just a holder
            AddComponent(e, new ProjectilePrefabRef
            { 
                Value = GetEntity(authoring.Prefab, TransformUsageFlags.Dynamic)
            });
        }
    }
}