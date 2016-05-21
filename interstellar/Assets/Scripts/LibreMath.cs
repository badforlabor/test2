
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
}
