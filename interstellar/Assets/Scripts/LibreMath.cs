
// 一些数学公式

using UnityEngine;
using System.Collections;

public static class LibreMath
{
    public static Quaternion ToQuaternion(Vector3 v)
    {
        //return Quaternion.LookRotation(v);
        return Quaternion.Euler(v);
    }
}
