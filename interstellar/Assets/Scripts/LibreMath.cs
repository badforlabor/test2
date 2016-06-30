
// 一些数学公式

using UnityEngine;
using System.Collections;

public static class LibreMath
{
    public static float Magnitude(Quaternion q)
    {
        float m = Mathf.Sqrt(q.x * q.x + q.y * q.y + q.z * q.z + q.w * q.w);
        return m;
    }
    public static Quaternion Normalize(Quaternion q)
    {
        float mag = Magnitude(q);
        return new Quaternion(q.x / mag, q.y / mag, q.z / mag, q.w / mag);
    }
    static float Rad2Deg (float rad)
    {
	    // TODO : should be rad * kRad2Deg, but can't be changed, 
	    // because it changes the order of operations and that affects a replay in some RegressionTests
	    return rad / 2.0F / Mathf.PI * 360.0F;
    }

    public static Quaternion ToQuaternion(Vector3 v)
    {
        //return Quaternion.LookRotation(v);
        return Quaternion.Euler(v);
    }
    public static Vector3 ToAngle(Quaternion q)
    {
//         Matrix4x4 m = Matrix4x4.TRS(Vector3.zero, q, Vector3.one);
//         return m.MultiplyVector(Vector3.forward);
        return q * Vector3.forward;
        //return q.eulerAngles;
    }
    public static Vector3 ToAngle2(Quaternion q)
    {
        return q * Vector3.forward;
    }

    public static Matrix4x4 ToCoordinate(Vector3 forward)
    {
        // 默认根据世界坐标系，求出forward对应的坐标系
        return ToCoordinate(forward, Vector3.up);
    }
    public static Matrix4x4 ToCoordinate(Vector3 forward, Vector3 upward)
    { 
        // 根据自己的朝向和父坐标系的Y轴，计算出自身坐标系

        // unity中z轴就是面向方向
        Vector3 z = forward.normalized;

        Vector3 x = Vector3.Cross(upward, z).normalized;

        Vector3 y = Vector3.Cross(z, x).normalized;

        Matrix4x4 matrix = new Matrix4x4();
        matrix.m00 = x.x; matrix.m01 = y.x; matrix.m02 = z.x;
        matrix.m10 = x.y; matrix.m11 = y.y; matrix.m12 = z.y;
        matrix.m20 = x.z; matrix.m21 = y.z; matrix.m22 = z.z;

        return matrix;
    }
}
