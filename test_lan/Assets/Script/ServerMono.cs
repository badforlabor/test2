﻿using UnityEngine;
using System;
using System.Collections;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Text;
using MyNet;

public class ServerMono : MonoBehaviour {

    // http://www.cnblogs.com/chenxizhang/archive/2011/09/10/2172994.html

    public int ServerPoint = 13131;
    string ServerIP;
    MyServerSocket MyServer = null;
	// Use this for initialization
	void Start () {
        MyServer = new MyServerSocket();
    }
	
	// Update is called once per frame
	void Update () {
        MyServer.Tick();
	}

    void OnGUI()
    {
        GUI.Label(new Rect(30, 30, 200, 30), "IP:" + ServerIP);
        if(GUI.Button(new Rect(30, 130, 100, 30), "启动"))
        {
            MyServer.Start(ServerPoint, OnReceiveMsg);
            ServerIP = MyServer.ServerAddr.Address.ToString();
        }
    }
    void OnReceiveMsg(int id, INetMessage msg)
    { 
    
    }
}
