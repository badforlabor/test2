using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;

namespace MyNet
{
    public interface INetMessage
    {
        void Decode(ByteReader reader);
        void Encode(ByteWriter writer);
        short GetID();
    }
    public static class NetCenter
    {
        static NetCenter()
        {
            Register(1, typeof(NetCommonMsg));
        }

        public delegate void ReceiveMsgCallback(int id, INetMessage msg);

        public static void Register(int id, Type msgType)
        {
            if (Dict.ContainsKey(id))
                Dict[id] = msgType;
            else
                Dict.Add(id, msgType);
        }
        public static void Send(Socket owner, INetMessage msg)
        {
            ByteWriter writer = new ByteWriter();
            writer.Write(msg.GetID());
            msg.Encode(writer);

            short cnt = (short)writer.GetBytes().Length;
            ByteWriter writer2 = new ByteWriter();
            writer2.Write(cnt);

            List<byte> ret = new List<byte>();
            ret.AddRange(writer2.GetBytes());
            ret.AddRange(writer.GetBytes());

            owner.Send(ret.ToArray());
        }
#if SYNC
#else
        public static int Dispatch(ByteReader reader, ReceiveMsgCallback callback)
        {
            if (callback == null)
                return 0;

            /*
             * 协议格式：
             *      short: 协议大小
             *      short: 协议号
             *      动态长度
             */
            int LastPos = 0;
            try
            {
                do
                {
                    int msg_len = reader.ReadShort();
                    int msg_start = reader.GetPos();

                    int id = reader.ReadShort();
                    INetMessage msg = NetCenter.Recognize(id, reader);
                    if (msg == null)
                        break;

                    // 协议大小不对!
                    if (msg_len != (reader.GetPos() - msg_start))
                        break;

                    LastPos = reader.GetPos();

                    try
                    {
                        callback(id, msg);
                    }
                    catch (Exception)
                    { }
                }
                while (!reader.IsEnd());
            }
            catch (Exception)
            { }

            return LastPos;
        }
#endif
        static Dictionary<int, Type> Dict = new Dictionary<int, Type>();
        public static INetMessage Recognize(int id, ByteReader reader)
        {
            if (!Dict.ContainsKey(id))
                return null;

            Type t = Dict[id];
            INetMessage msg = (INetMessage)System.Activator.CreateInstance(t, null);

            msg.Decode(reader);

            return msg;
        }

    }
}
