using UnityEngine;
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
    public string IP = "192.168.2.160";
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

    void OnDestroy()
    {
        MyServer.Destroy();
    }

    void OnGUI()
    {
        GUI.Label(new Rect(30, 30, 200, 30), "IP:" + ServerIP);
        if(GUI.Button(new Rect(30, 130, 100, 30), "启动"))
        {
            MyServer.Start(IP, ServerPoint, OnReceiveMsg);
            ServerIP = MyServer.ServerAddr.Address.ToString();
        }
        if (GUI.Button(new Rect(30, 230, 100, 30), "断开连接"))
        {
            MyServer.SimulateDisconnect();
        }
    }
    void OnReceiveMsg(int id, INetMessage arg)
    {
        NetCommonMsg msg = arg as NetCommonMsg;
        Debug.Log("OnReceiveMsg: id=" + id + ", message=" + msg.msg);
    }
}
