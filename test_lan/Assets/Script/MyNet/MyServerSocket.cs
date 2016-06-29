#define Async
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

        Semaphore semaphore = new Semaphore(1, 1);

        public void Start(string ip, int port, NetCenter.ReceiveMsgCallback callback)
        {
            Callback = callback;

            // 启动服务器
            //IPAddress[] ipaddress = Dns.GetHostAddresses(Dns.GetHostName());
            //ipaddress[0]
            ServerAddr = new IPEndPoint(IPAddress.Parse(ip), port);

            ServerSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            ServerSocket.Bind(ServerAddr);
            ServerSocket.Listen(1);

            ServerSocket.BeginAccept(ClientAccept, ServerSocket);
        }
        public void Tick()
        {
            //if (ClientSocket != null)
            {
                TickReceive();
            }
        }
        public void Destroy()
        {
            if (ClientSocket != null)
                ClientSocket.Close();
            if(ServerSocket != null)
                ServerSocket.Close();            
        }
        byte[] buffer = new byte[1024];
        List<byte> ReceivedBuffer = new List<byte>();

        void TickReceive()
        {
            int cnt = 0;
            semaphore.WaitOne();
            cnt = ReceivedBuffer.Count;
            semaphore.Release();

            if (cnt == 0)
                return;

            semaphore.WaitOne();
            ByteReader reader = new ByteReader(ReceivedBuffer.ToArray(), 0);
            int LastPos = NetCenter.Dispatch(reader, Callback);
            if (LastPos > 0)
            {
                ReceivedBuffer.RemoveRange(0, LastPos);
            }
            semaphore.Release();
        }
        void ClientAccept(IAsyncResult ar)
        {
            var server = ar.AsyncState as Socket;
            var client = server.EndAccept(ar);
            if (ClientSocket == null)
            {
                ClientSocket = client;
                client.BeginReceive(buffer, 0, buffer.Length, SocketFlags.None, OnReceivewMsg, client);
            }
        }
        void OnReceivewMsg(IAsyncResult ar)
        {
            try
            {
                var client = ar.AsyncState as Socket;
                var length = client.EndReceive(ar);

                semaphore.WaitOne();
                for (int i = 0; i < length; i++)
                    ReceivedBuffer.Add(buffer[i]);
                semaphore.Release();                

                client.BeginReceive(buffer, 0, buffer.Length, SocketFlags.None, OnReceivewMsg, client);
            }
            catch (Exception)
            { }
        }
    }
}
