using Unity.Entities;
using Unity.Entities.UniversalDelegates;
using UnityEngine;

public class SpawnerAuthoring : MonoBehaviour
{
    public int MinSpawnCount = 5;
    public int MaxSpawnCount = 10;

    public float BaseInterval = 5;    // starting spawn interval
    public float MinInterval = 0.3f;     // cap so it doesn't get insane
    public float RampRate = 0.1f;        // how fast interval shrinks over time
    public float SpawnRadius = 20f;     // inner edge of the band
    public float SpawnBandWidth = 5f;  // how wide the band is
    public GameObject EnemyPrefab;

    class Baker : Baker<SpawnerAuthoring>
    {
        public override void Bake(SpawnerAuthoring authoring)
        {
            var e = GetEntity(TransformUsageFlags.None);
            AddComponent(e, new SpawnerData{
                Rng = Unity.Mathematics.Random.CreateFromIndex(1234),
                Timer = authoring.BaseInterval,  
                MinSpawnCount = authoring.MinSpawnCount,
                MaxSpawnCount = authoring.MaxSpawnCount,
                BaseInterval = authoring.BaseInterval, 
                MinInterval = authoring.MinInterval, 
                RampRate = authoring.RampRate, 
                SpawnRadius = authoring.SpawnRadius, 
                SpawnBandWidth = authoring.SpawnBandWidth,
                EnemyPrefab = GetEntity(authoring.EnemyPrefab, TransformUsageFlags.Dynamic)
            });
        }
    }
}