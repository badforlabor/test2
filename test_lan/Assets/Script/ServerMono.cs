using UnityEngine;
using System;
using System.Collections;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Text;

public class ServerMono : MonoBehaviour {

    // http://www.cnblogs.com/chenxizhang/archive/2011/09/10/2172994.html

    string ServerIP = "";
    int ServerPoint = 13131;
    Socket ServerSocket = null;

	// Use this for initialization
	void Start () {

        // 启动服务器
        IPAddress[] ipaddress = Dns.GetHostAddresses(Dns.GetHostName());
        ServerIP = ipaddress[0].ToString();
        Debug.Log("server ip=" + ServerIP);

        ServerSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        ServerSocket.Bind(new IPEndPoint(ipaddress[0], ServerPoint));
        ServerSocket.Listen(4);

        ServerSocket.BeginAccept(new AsyncCallback(ClientAccepted), ServerSocket);

	}
    void ClientAccepted(IAsyncResult ar)
    {
        var socket = ar.AsyncState as Socket;
        var client = socket.EndAccept(ar);

        client.Send(System.Text.Encoding.UTF8.GetBytes("你好，客户端"));

        client.BeginReceive(buffer, 0, buffer.Length, SocketFlags.None, new AsyncCallback(ReceiveMessage), client);
    }
    byte[] buffer = new byte[1024];
    void ReceiveMessage(IAsyncResult ar)
    {
        var client = ar.AsyncState as Socket;
        var length = client.EndReceive(ar);
        var message = System.Text.Encoding.UTF8.GetString(buffer, 0, length);
    }
    
	
	// Update is called once per frame
	void Update () {
	
	}
}
