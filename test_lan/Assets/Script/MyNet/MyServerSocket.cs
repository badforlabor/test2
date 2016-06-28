using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Text;

namespace MyNet
{
    class MyServerSocket
    {
        public Socket ServerSocket;
        public Socket ClientSocket;

        NetCenter.ReceiveMsgCallback Callback;
        public IPEndPoint ServerAddr;

        Thread AcceptThread = null;
        bool bStop = false;

        public void Start(int port, NetCenter.ReceiveMsgCallback callback)
        {
            Callback = callback;

            // 启动服务器
            IPAddress[] ipaddress = Dns.GetHostAddresses(Dns.GetHostName());
            ServerAddr = new IPEndPoint(ipaddress[0], port);

            ServerSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            ServerSocket.Bind(ServerAddr);
            ServerSocket.Listen(4);

            AcceptThread = new Thread(delegate()
                {
                    while (!bStop)
                    {
                        try
                        {
                            Socket socket = ServerSocket.Accept();
                            if (socket != null)
                            {
                                ClientSocket = socket;
                            }
                        }
                        catch (Exception)
                        {

                        }
                        Thread.Sleep(100);
                    }
                });
            AcceptThread.Start();
        }
        public void Tick()
        {
            if (ClientSocket != null)
            {
                TickReceive();
            }
        }
        byte[] buffer = new byte[1024];
        List<byte> ReceivedBuffer = new List<byte>();
        void TickReceive()
        {
            if (!ClientSocket.Connected)
                return;
            // 如果发现断开连接了，返回

            int length = ClientSocket.Receive(buffer);
            if (length == 0)
                return;

            ReceivedBuffer.Clear();
            do
            {
                ReceivedBuffer.AddRange(buffer);
                length = ClientSocket.Receive(buffer);
            }
            while (length > 0);

            // 解析
            NetCenter.Dispatch(ReceivedBuffer.ToArray(), Callback);
        }
    }
}
