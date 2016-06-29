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

        string SavedIP = "";
        int SavedPort = 0;
        public void Start(string ip, int port, NetCenter.ReceiveMsgCallback callback)
        {
            if (Status != 0)
                return;

            SavedIP = ip;
            SavedPort = port;

            Status = 1;
            Callback = callback;
            Client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            Client.Connect(ip, port);

            DoAsyncReceive();
        }
        public void Tick()
        {
            TickReceive();

            if (Client != null && !Client.Connected)
            {
                SimulateDisconnect();
                Reconn();
            }

            // 自动重连
            if (Client == null && Status == 1)
            {
                Reconn();
            }
        }
        public void Destroy()
        {
            if (Client != null)
            {
                Client.Shutdown(SocketShutdown.Both);
                Client.Close();
                Client = null;
            }
            Status = 0;
        }
        public void SendMessage(string msg)
        {
            NetCommonMsg proto = new NetCommonMsg();
            proto.msg = msg;
            NetCenter.Send(Client, proto);
        }
        public void Reconn()
        {
            Client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            Client.Connect(SavedIP, SavedPort);
            DoAsyncReceive();
        }
        // 模拟断开连接
        public void SimulateDisconnect()
        {
            Client.Shutdown(SocketShutdown.Both);
            Client.Disconnect(false);
            Client = null;
        }

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

                if (length > 0)
                {
                    semaphore.WaitOne();
                    for (int i = 0; i < length; i++)
                        ReceivedBuffer.Add(buffer[i]);
                    semaphore.Release();

                    //接收下一个消息(因为这是一个递归的调用，所以这样就可以一直接收消息了）
                    client.BeginReceive(buffer, 0, buffer.Length, SocketFlags.None, new AsyncCallback(ReceiveMessage), client);
                }
                else
                {
                    // 服务器主动与自己关闭的时候
                    client.Close();
                    Client = null;
                }

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
    }
}
