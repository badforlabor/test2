using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MyNet
{
    public interface INetMessage
    {
        void Decode(ByteReader reader);
    }
    public static class NetCenter
    {
        public delegate void ReceiveMsgCallback(int id, INetMessage msg);

        public static void Register(int id, Type msgType)
        {
            if (Dict.ContainsKey(id))
                Dict[id] = msgType;
            else
                Dict.Add(id, msgType);
        }
        public static void Dispatch(byte[] buffer, ReceiveMsgCallback callback)
        {
            if (callback == null)
                return;

            /*
             * 协议格式：
             *      short: 协议号
             *      short: 协议大小
             *      动态长度
             */
            ByteReader reader = new ByteReader(buffer, 0);
            try
            {
                do
                {
                    int start = reader.GetPos();
                    int id = reader.ReadShort();
                    int msg_len = reader.ReadShort();
                    INetMessage msg = Recognize(id, reader);
                    if (msg == null)
                        break;

                    // 协议大小不对！
                    if (msg_len != (reader.GetPos() - start))
                        break;

                    callback(id, msg);
                }
                while (!reader.IsEnd());
            }
            catch (Exception)
            { }
        }

        static Dictionary<int, Type> Dict = new Dictionary<int, Type>();
        static INetMessage Recognize(int id, ByteReader reader)
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
