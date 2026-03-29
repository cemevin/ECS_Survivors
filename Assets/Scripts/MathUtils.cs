using Unity.Mathematics;

public static class MathUtils
{
    public static float3 RotateAroundY(float3 dir, float angle)
    {
        float cos = math.cos(angle);
        float sin = math.sin(angle);
        return new float3(
            dir.x * cos + dir.z * sin,
            0,
            -dir.x * sin + dir.z * cos
        );
    }
}