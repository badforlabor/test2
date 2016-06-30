using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class bazier2 : MonoBehaviour {

    public float Granularity = 50;
    List<Vector3> points = new List<Vector3>();
    List<Vector3> points_front = new List<Vector3>();

    public Transform StartPoint;
    public Transform EndPoint;

    public float HSpeed;
    public float VSpeed;
    public float LifeTime;

    public Vector3 P1, P2;

    Quaternion NewRotation = Quaternion.identity;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    void OnDrawGizmos()
    {
        if (points_front.Count > 0)
        {
            Color c = Gizmos.color;
            Gizmos.color = Color.red;
            for (int i = 0; i < points_front.Count - 1; i++)
            {
                Gizmos.DrawLine(points_front[i], points_front[i + 1]);
            }
            UnityEditor.Handles.ArrowCap(0, points_front[points_front.Count - 1], NewRotation, 3);
            Gizmos.color = c;
        }
        for (int i = 0; i < points.Count - 1; i++)
        {
            Gizmos.DrawLine(points[i], points[i + 1]);
        }
    }

    public void Refresh()
    {
        Debug.Log("xxxxxxxxxxxx refresh bazier2.");

        if (StartPoint == null || EndPoint == null)
        {
            return;
        }

        Vector3 newStartPoint = StartPoint.transform.position;
        Vector3 newEndPoint = EndPoint.transform.position;

        // 计算前半段曲线
        points_front.Clear();
        if (LifeTime > 0)
        {
            points_front.Add(newStartPoint);
            Vector3 temp = newStartPoint + (StartPoint.transform.forward * HSpeed * LifeTime) + (Vector3.up * VSpeed * LifeTime);
            NewRotation = Quaternion.LookRotation((temp - newStartPoint).normalized);
            points_front.Add(temp);

            newStartPoint = temp;
        }
        else
        {
            NewRotation = this.transform.rotation;
            if (NewRotation != Quaternion.LookRotation(this.transform.forward))
            {
                Debug.Log("xxxxxxxxxxxx error!");
            }
        }

        // 计算P1， P2的值            
        Vector3 p1 = P1;
        Vector3 BazierP1 = Matrix4x4.TRS(newStartPoint, NewRotation, Vector3.one).MultiplyPoint(p1);

        Vector3 p2 = P2;
        Vector3 BazierP2 = Matrix4x4.TRS(newEndPoint, NewRotation, Vector3.one).MultiplyPoint(p2);

        // 计算后半段曲线
        points.Clear();
        float p = 1.0f / Granularity;
        float i = 0;
        while (i < 1.0f)
        {
            points.Add(bazier.CalculateBezier(i, newStartPoint, BazierP1, BazierP2, newEndPoint));
            i += p;
        }
    }
}
