using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MyNet
{
    public class NetCommonMsg : INetMessage
    {
        public string msg;
        public void Decode(ByteReader reader)
        {
            msg = reader.ReadString();
        }
        public void Encode(ByteWriter writer)
        {
            writer.Write(msg);
        }
        public short GetID()
        {
            return 1;
        }
    }
}
