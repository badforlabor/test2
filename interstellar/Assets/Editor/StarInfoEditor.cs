using UnityEngine;
using UnityEditor;
using System.Collections;

[CustomEditor(typeof(StarInfo))]
public class StarInfoEditor : Editor 
{
    void OnSceneGUI()
    {
        StarInfo go = target as StarInfo;
        if (Selection.Contains(go.gameObject))
        {

        }
        else
        {
            Handles.ArrowCap(0, go.transform.position, go.transform.rotation, 1);
        }
    }

}
