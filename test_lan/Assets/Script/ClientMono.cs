using UnityEngine;
using System;
using System.Collections;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Text;
using MyNet;

public class ClientMono : MonoBehaviour {

    public int ServerPort = 13131;
    public string ServerIp = "localhost";

    MyClientSocket MyClient = null;

	// Use this for initialization
	void Start () {
        
	}
	// Update is called once per frame
	void Update () {
        if (MyClient != null)
        {
            MyClient.Tick();
        }
	}
    void OnDestory()
    {
        if (MyClient != null)
        {
            MyClient.Destroy();
        }
    }

    void OnGUI()
    {
        if (GUI.Button(new Rect(Screen.width - 150, 30, 50, 20), "连接"))
        {
            if (MyClient == null)
            {
                MyClient = new MyClientSocket();
            }
            MyClient.Start(ServerIp, ServerPort, OnReceiveMsg);
        }
    }
    void OnReceiveMsg(int id, INetMessage msg)
    { 
    
    }
}
