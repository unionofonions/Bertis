using System.Runtime.CompilerServices;
using UnityEngine;

namespace Clockwork;

public static class Math
{
    public const float Rad2Deg = 57.29578f;

    public const float Deg2Rad = 0.017453f;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe float AsSingle(uint value)
        => *(float*)&value;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe uint AsUInt32(float value)
        => *(uint*)&value;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int Min(int left, int right)
        => left <= right ? left : right;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float Min(float left, float right)
        => (float.IsNaN(right) || left <= right) ? left : right;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int Max(int left, int right)
        => left >= right ? left : right;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float Max(float left, float right)
        => (float.IsNaN(right) || left >= right) ? left : right;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int Abs(int value)
        => Max(-value, value);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float Abs(float value)
        => AsSingle(AsUInt32(value) & 0x7FFFFFFFu);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int Sign(int value)
        => unchecked(value >> 31 | (int)((uint)-value >> 31));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float Sign(float value)
        => value < 0f ? -1f : value > 0f ? 1f : value;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector2 Sign(Vector2 value)
        => new(Sign(value.x), Sign(value.y));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector3 Sign(Vector3 value)
        => new(Sign(value.x), Sign(value.y), Sign(value.z));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int Clamp(int value, int min, int max)
        => value < min ? min : value > max ? max : value;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float Clamp(float value, float min, float max)
        => value < min ? min : value > max ? max : value;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector2 Clamp(Vector2 value, Vector2 min, Vector2 max)
        => new(
            Clamp(value.x, min.x, max.x),
            Clamp(value.y, min.y, max.y));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector3 Clamp(Vector3 value, Vector3 min, Vector3 max)
        => new(
            Clamp(value.x, min.x, max.x),
            Clamp(value.y, min.y, max.y),
            Clamp(value.z, min.z, max.z));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float Clamp01(float value)
        => value < 0f ? 0f : value > 1f ? 1f : value;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float Sqrt(float value)
        => (float)System.Math.Sqrt(value);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float Sin(float value)
        => (float)System.Math.Sin(value);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float Cos(float value)
        => (float)System.Math.Cos(value);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static uint Rol(uint value, int offset)
        => (value << offset) | (value >> (32 - offset));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ulong Rol(ulong value, int offset)
        => (value << offset) | (value >> (64 - offset));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static uint Ror(uint value, int offset)
        => (value >> offset) | (value << (32 - offset));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ulong Ror(ulong value, int offset)
        => (value >> offset) | (value << (64 - offset));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static uint NextPow2(uint value)
    {
        --value;
        value |= value >> 1;
        value |= value >> 2;
        value |= value >> 4;
        value |= value >> 8;
        value |= value >> 16;
        return value + 1;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ulong NextPow2(ulong value)
    {
        --value;
        value |= value >> 1;
        value |= value >> 2;
        value |= value >> 4;
        value |= value >> 8;
        value |= value >> 16;
        value |= value >> 32;
        return value + 1;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Quaternion FromEuler(Vector3 value)
        => Quaternion.Euler(value);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector3 ToEuler(Quaternion value)
        => value.eulerAngles;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float Length(Vector2 value)
        => Sqrt(Dot(value, value));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float Length(Vector3 value)
        => Sqrt(Dot(value, value));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float LengthSq(Vector2 value)
        => Dot(value, value);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float LengthSq(Vector3 value)
        => Dot(value, value);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float Csum(Vector2 value)
        => value.x + value.y;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float Csum(Vector3 value)
        => value.x + value.y + value.z;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector3 Mul(Vector3 left, Vector3 right)
        => new(left.x * right.x, left.y * right.y, left.z * right.z);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float Dot(Vector2 left, Vector2 right)
        => left.x * right.x + left.y * right.y;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float Dot(Vector3 left, Vector3 right)
        => left.x * right.x + left.y * right.y + left.z * right.z;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float Distance(Vector3 left, Vector3 right)
        => Length(right - left);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float Distance(Vector2 left, Vector2 right)
        => Length(right - left);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Quaternion Conjugate(Quaternion value)
        => new(-value.x, -value.y, -value.z, value.w);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float Lerp(float source, float target, float progress)
        => source + (target - source) * progress;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector2 Lerp(Vector2 source, Vector2 target, float progress)
        => source + (target - source) * progress;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector3 Lerp(Vector3 source, Vector3 target, float progress)
        => source + (target - source) * progress;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Quaternion Nlerp(Quaternion source, Quaternion target, float progress)
        => Quaternion.LerpUnclamped(source, target, progress);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Quaternion Slerp(Quaternion source, Quaternion target, float progress)
        => Quaternion.SlerpUnclamped(source, target, progress);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float LerpSnap(float source, float target, float progress, float snapThreshold)
        => Abs(target - source) <= snapThreshold ? target : Lerp(source, target, progress);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float Unlerp(float source, float target, float value)
        => source == target ? 1f : (value - source) / (target - source);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool Nearby(Vector3 source, Vector3 target, float distance)
        => LengthSq(source - target) <= distance * distance;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector2 Deadzone(Vector2 value, float threshold)
        => LengthSq(value) >= threshold * threshold ? value : Vector2.zero;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector3 Deadzone(Vector3 value, float threshold)
        => LengthSq(value) >= threshold * threshold ? value : Vector3.zero;

    public static float SmoothDamp(
        float current,
        float target,
        ref float velocity,
        float smoothness,
        float deltaTime,
        float maxSpeed = float.PositiveInfinity)
    {
        smoothness = Max(0.0001f, smoothness);

        float omega = 2f / smoothness;
        float x = omega * deltaTime;
        float exp = 1f / (1f + x + 0.48f * x * x + 0.235f * x * x * x);
        float change = current - target;

        float maxChange = maxSpeed * smoothness;
        change = Clamp(change, -maxChange, maxChange);

        float temp = (velocity + omega * change) * deltaTime;
        float result = current - change + (change + temp) * exp;

        if ((target - current > 0f) == (result > target))
        {
            result = target;
            velocity = (result - target) / deltaTime;
        }
        else
        {
            velocity = (velocity - omega * temp) * exp;
        }

        return result;
    }

    public static Vector2 SmoothDamp(
        Vector2 current,
        Vector2 target,
        ref Vector2 velocity,
        float smoothness,
        float deltaTime,
        float maxSpeed = float.PositiveInfinity)
    {
        smoothness = Max(0.0001f, smoothness);

        float omega = 2f / smoothness;
        float x = omega * deltaTime;
        float exp = 1f / (1f + x + 0.48f * x * x + 0.235f * x * x * x);
        float changeX = current.x - target.x;
        float changeY = current.y - target.y;

        float maxChange = maxSpeed * smoothness;
        float maxChangeSq = maxChange * maxChange;
        float disSq = changeX * changeX + changeY * changeY;

        if (disSq > maxChangeSq)
        {
            float dis = Sqrt(disSq);
            changeX = changeX / dis * maxChange;
            changeY = changeY / dis * maxChange;
        }

        float tempX = (velocity.x + omega * changeX) * deltaTime;
        float tempY = (velocity.y + omega * changeY) * deltaTime;

        float resultX = current.x - changeX + (changeX + tempX) * exp;
        float resultY = current.y - changeY + (changeY + tempY) * exp;

        float t1x = target.x - current.x;
        float t1y = target.y - current.y;
        float t2x = resultX - target.x;
        float t2y = resultY - target.y;

        if (t1x * t2x + t1y * t2y > 0f)
        {
            resultX = target.x;
            resultY = target.y;
            velocity.x = (resultX - target.x) / deltaTime;
            velocity.y = (resultY - target.y) / deltaTime;
        }
        else
        {
            velocity.x = (velocity.x - omega * tempX) * exp;
            velocity.y = (velocity.y - omega * tempY) * exp;
        }

        return new(resultX, resultY);
    }

    public static Vector3 SmoothDamp(
        Vector3 current,
        Vector3 target,
        ref Vector3 velocity,
        float smoothness,
        float deltaTime,
        float maxSpeed = float.PositiveInfinity)
    {
        smoothness = Max(0.0001f, smoothness);

        float omega = 2f / smoothness;
        float x = omega * deltaTime;
        float exp = 1f / (1f + x + 0.48f * x * x + 0.235f * x * x * x);
        float changeX = current.x - target.x;
        float changeY = current.y - target.y;
        float changeZ = current.z - target.z;

        float maxChange = maxSpeed * smoothness;
        float maxChangeSq = maxChange * maxChange;
        float disSq = changeX * changeX + changeY * changeY + changeZ * changeZ;

        if (disSq > maxChangeSq)
        {
            float dis = Sqrt(disSq);
            changeX = changeX / dis * maxChange;
            changeY = changeY / dis * maxChange;
            changeZ = changeZ / dis * maxChange;
        }

        float tempX = (velocity.x + omega * changeX) * deltaTime;
        float tempY = (velocity.y + omega * changeY) * deltaTime;
        float tempZ = (velocity.z + omega * changeZ) * deltaTime;

        float resultX = current.x - changeX + (changeX + tempX) * exp;
        float resultY = current.y - changeY + (changeY + tempY) * exp;
        float resultZ = current.z - changeZ + (changeZ + tempZ) * exp;

        float t1x = target.x - current.x;
        float t1y = target.y - current.y;
        float t1z = target.z - current.z;
        float t2x = resultX - target.x;
        float t2y = resultY - target.y;
        float t2z = resultZ - target.z;

        if (t1x * t2x + t1y * t2y + t1z * t2z > 0f)
        {
            resultX = target.x;
            resultY = target.y;
            resultZ = target.z;
            velocity.x = (resultX - target.x) / deltaTime;
            velocity.y = (resultY - target.y) / deltaTime;
            velocity.z = (resultZ - target.z) / deltaTime;
        }
        else
        {
            velocity.x = (velocity.x - omega * tempX) * exp;
            velocity.y = (velocity.y - omega * tempY) * exp;
            velocity.z = (velocity.z - omega * tempZ) * exp;
        }

        return new(resultX, resultY, resultZ);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float Perlin(float x, float y)
        => UnityEngine.Mathf.PerlinNoise(x, y);
}