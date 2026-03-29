using Unity.Entities;
using UnityEngine;

public class HealthBarAuthoring : MonoBehaviour
{
    class Baker : Baker<HealthBarAuthoring>
    {
        public override void Bake(HealthBarAuthoring authoring)
        {
            var e = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(e, new HealthBarFill { Value = 1f });
            AddComponent(e, new HealthBarOwner 
            { 
                EnemyEntity = GetEntity(authoring.transform.parent.gameObject, 
                                    TransformUsageFlags.Dynamic)
            });
        }
    }
}