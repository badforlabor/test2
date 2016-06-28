using UnityEngine;
using System;
using System.Collections;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Text;

public class ClientMono : MonoBehaviour {

    public int Port = 13131;

	// Use this for initialization
	void Start () {
        var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        socket.Connect("localhost", Port);
        socket.BeginReceive(buffer, 0, buffer.Length, SocketFlags.None, new AsyncCallback(ReceiveMessage), socket);
	}
    void ReceiveMessage(IAsyncResult ar)
    {
        try
        {
            var socket = ar.AsyncState as Socket;

            //方法参考：http://msdn.microsoft.com/zh-cn/library/system.net.sockets.socket.endreceive.aspx
            var length = socket.EndReceive(ar);
            //读取出来消息内容
            var message = Encoding.Unicode.GetString(buffer, 0, length);
            //显示消息
            //Console.WriteLine(message);

            //接收下一个消息(因为这是一个递归的调用，所以这样就可以一直接收消息了）
            socket.BeginReceive(buffer, 0, buffer.Length, SocketFlags.None, new AsyncCallback(ReceiveMessage), socket);
        }
        catch (Exception ex)
        {
            //Console.WriteLine(ex.Message);
        }
    
    }
    byte[] buffer = new byte[1024];
	// Update is called once per frame
	void Update () {
	
	}
}
