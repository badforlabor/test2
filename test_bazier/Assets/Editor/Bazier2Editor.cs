/****************************************************************************
Copyright (c) 2013-2014,Dalian-LingYOU tech.
 This is not a free-ware .DO NOT use it without any authorization.
 * 
 * date : 2016/6/16 10:00:30
 * author : Labor
 * purpose : 
****************************************************************************/
using UnityEngine;
using UnityEditor;
using System;
using System.Collections;
using System.Collections.Generic;

namespace MLGame
{
    [CustomEditor(typeof(bazier2))]
    class Bazier2Editor : Editor
    {
        public override void OnInspectorGUI()
        {
            this.DrawDefaultInspector();
            
            bazier2 b = target as bazier2;
            // 两者之间的距离
            if(b.StartPoint != null && b.EndPoint != null)
            {
                GUILayout.Label("两者之间的距离：" + (b.EndPoint.transform.position - b.StartPoint.transform.position).magnitude + "米");
            }
            
            if (GUILayout.Button("refresh"))
            {
                b.Refresh();
            }
        }   
    }
}
