using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Text;

namespace MyNet
{
    class MyClientSocket
    {
        public Socket Client;

        int Status = 0;
        NetCenter.ReceiveMsgCallback Callback;
        byte[] buffer = new byte[1024];
        List<byte> ReceivedBuffer = new List<byte>();

        Thread AcceptThread = null;
        bool bStop = false;
        bool bReady = false;

        public void Start(string ip, int port, NetCenter.ReceiveMsgCallback callback)
        {
            if (Status != 0)
                return;

            Status = 1;
            Callback = callback;
            Client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            
            AcceptThread = new Thread(delegate()
            {
                Client.Connect(ip, port);
                bReady = true;
            });
            AcceptThread.Start();
        }
        public void Tick()
        {
            TickReceive();
        }
        public void Destroy()
        {
            bStop = false;
            if (Client != null)
            {
                Client.Close();
            }
        }
#if ASYNC
        void DoAsyncReceive()
        {
            Client.BeginReceive(buffer, 0, buffer.Length, SocketFlags.None, new AsyncCallback(ReceiveMessage), Client);
        }

        void ReceiveMessage(IAsyncResult ar)
        {
            try
            {
                var socket = ar.AsyncState as Socket;

                //方法参考：http://msdn.microsoft.com/zh-cn/library/system.net.sockets.socket.endreceive.aspx
                var length = socket.EndReceive(ar);
                if (length > 0)
                { 
                    
                }

                //接收下一个消息(因为这是一个递归的调用，所以这样就可以一直接收消息了）
                socket.BeginReceive(buffer, 0, buffer.Length, SocketFlags.None, new AsyncCallback(ReceiveMessage), socket);
            }
            catch (Exception)
            {
                //Console.WriteLine(ex.Message);
            }

        }
#else
        void DoAsyncReceive()
        { }
        void TickReceive()
        {
            if (!bReady)
                return;

            // 如果发现断开连接了，那么尝试重练，返回
            if (!Client.Connected)
                return;

            int length = Client.Receive(buffer);
            if (length == 0)
                return;

            ReceivedBuffer.Clear();
            do
            {
                ReceivedBuffer.AddRange(buffer);
                length = Client.Receive(buffer);
            }
            while (length > 0);

            // 解析
            NetCenter.Dispatch(ReceivedBuffer.ToArray(), Callback);
        }
#endif
    }
}
