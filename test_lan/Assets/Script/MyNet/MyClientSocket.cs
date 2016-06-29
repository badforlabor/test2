#define ASYNC
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

        Semaphore semaphore = new Semaphore(1, 1);

        public void Start(string ip, int port, NetCenter.ReceiveMsgCallback callback)
        {
            if (Status != 0)
                return;

            Status = 1;
            Callback = callback;
            Client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            Client.Connect(ip, port);

            DoAsyncReceive();
        }
        public void Tick()
        {
            TickReceive();
        }
        public void Destroy()
        {
            if (Client != null)
            {
                Client.Close();
            }
        }
        public void SendMessage(string msg)
        {
            NetCommonMsg proto = new NetCommonMsg();
            proto.msg = msg;
            NetCenter.Send(Client, proto);
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
                var client = ar.AsyncState as Socket;
                var length = client.EndReceive(ar);

                semaphore.WaitOne();
                for (int i = 0; i < length; i++)
                    ReceivedBuffer.Add(buffer[i]);
                semaphore.Release();          

                //接收下一个消息(因为这是一个递归的调用，所以这样就可以一直接收消息了）
                client.BeginReceive(buffer, 0, buffer.Length, SocketFlags.None, new AsyncCallback(ReceiveMessage), client);
            }
            catch (Exception)
            {
                //Console.WriteLine(ex.Message);
            }
        }
        void TickReceive()
        {
            int cnt = 0;
            semaphore.WaitOne();
            cnt = ReceivedBuffer.Count;
            semaphore.Release();

            if (cnt == 0)
            {
                return;
            }

            semaphore.WaitOne();
            ByteReader reader = new ByteReader(ReceivedBuffer.ToArray(), 0);
            int LastPos = NetCenter.Dispatch(reader, Callback);
            if (LastPos > 0)
            {
                ReceivedBuffer.RemoveRange(0, LastPos);
            }
            semaphore.Release();
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
