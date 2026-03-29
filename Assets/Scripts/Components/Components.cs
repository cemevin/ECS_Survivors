using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

// Tags (zero-size, just for query filtering)
public struct EnemyTag : IComponentData { }
public struct ProjectileTag : IComponentData { }
public struct AoeTag : IComponentData { }
public struct ShockwaveTag : IComponentData { }
public struct PlayerTag : IComponentData { }

public struct Direction : IComponentData
{
    public float3 Value;
}


public struct Health : IComponentData
{
    public float Current;
    public float Max;
}

public struct Damage : IComponentData
{
    public float Value;
}

public struct ImpulseOnHit : IComponentData
{
    public float Value;
}

public struct Lifetime : IComponentData
{
    public float Remaining;
    public float Total;
}

public struct MoveSpeed : IComponentData
{
    public float Value;
}

public struct ShockwaveData : IComponentData
{
    public float CurrentRadius;
    public float MaxRadius;
    public float PushForce;
    public float ExpandSpeed;
}

public struct ImpulseVelocity : IComponentData
{
    public float3 Value;
}
public struct PlayerInput : IComponentData
{
    public float3 MoveDirection;
    public bool FirePressed;
    public bool ShockwavePressed;
}

public struct WeaponData : IComponentData
{
    public float FireRate;     // seconds between shots
    public float Timer;        // counts down to 0 then resets
    public int ProjectileCount; // how many per burst, e.g. 8 for circular spread
}
public struct ProjectilePrefabRef : IComponentData
{
    public Entity Value;
}
public struct PrefabScale : IComponentData
{
    public float Value;
}

public struct AimDirection : IComponentData
{
    public float3 Value;
}
 
public struct SpawnerData : IComponentData
{
    public Unity.Mathematics.Random Rng;
    public float Timer;

    public int MinSpawnCount;
    public int MaxSpawnCount;
    public float BaseInterval;    // starting spawn interval
    public float MinInterval;     // cap so it doesn't get insane
    public float RampRate;        // how fast interval shrinks over time
    public float SpawnRadius;     // inner edge of the band
    public float SpawnBandWidth;  // how wide the band is
    public Entity EnemyPrefab;

}