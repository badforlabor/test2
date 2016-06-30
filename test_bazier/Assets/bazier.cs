using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class bazier : MonoBehaviour {

    public float Granularity = 50;
    public Vector3 StartPos;
    public Vector3 EndPos;
    public Vector3 p1, p2;

    List<Vector3> points = new List<Vector3>();
    

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    void OnDrawGizmos()
    {
        for (int i = 0; i < points.Count-1; i++)
        {
            Gizmos.DrawLine(points[i], points[i + 1]);
        }
    }

    public void Refresh()
    {
        Debug.Log("xxxxxxxxxxxx refresh bazier.");

        points.Clear();
        float p = 1.0f / Granularity;
        float i = 0;
        while (i < 1.0f)
        {
            points.Add(CalculateBezier(i, StartPos, p1, p2, EndPos));
            i += p;
        }        
    }

    //t - the time (0-1) of the curve to sample
    //p - the start point of the curve
    //a - control point from p
    //b - control point from q
    //q - the end point of the curve
    public static Vector3 CalculateBezier(float t, Vector3 p, Vector3 a, Vector3 b, Vector3 q)
    {
        float t2 = t * t;
        float t3 = t2 * t;
        float u = 1.0f - t;
        float u2 = u * u;
        float u3 = u2 * u;

        Vector3 output = u3 * p + 3 * u2 * t * a + 3 * u * t2 * b + t3 * q;

        return output;
    }
}
