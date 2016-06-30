using UnityEngine;
using UnityEditor;
using System.Collections;

[CustomEditor(typeof(bazier))]
public class BazierEditor : Editor
{
    public override void OnInspectorGUI()
    {
        this.DrawDefaultInspector();

        //if (GUI.changed)
        if(GUILayout.Button("refresh"))
        { 
            bazier b = target as bazier;
            b.Refresh();
        }
    }   
}