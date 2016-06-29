using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MyNet
{
    class NetSerialize
    {
    }
    public class ByteReader
    {
        byte[] Buffer;
        int Pos;

        public ByteReader(byte[] buffer, int start)
        {
            Buffer = buffer;
            Pos = start;
        }
        public short ReadShort()
        {
            if (IsEnd())
            {
                throw new Exception("error buffer");
            }
            short v = BitConverter.ToInt16(Buffer, Pos);
            Pos += sizeof(short);
            return v;
        }
        public int ReadInt()
        {
            if (IsEnd())
            {
                throw new Exception("error buffer");
            }
            int v = BitConverter.ToInt32(Buffer, Pos);
            Pos += sizeof(int);
            return v;
        }
        public float ReadFloat()
        {
            if (IsEnd())
            {
                throw new Exception("error buffer");
            }
            float v = BitConverter.ToSingle(Buffer, Pos);
            Pos += sizeof(float);
            return v;
        }
        public string ReadString()
        {
            if (IsEnd())
            {
                throw new Exception("error buffer");
            }
            int len = BitConverter.ToInt32(Buffer, Pos);
            Pos += sizeof(int);

            if (IsEnd())
            {
                throw new Exception("error buffer");
            }
            string v = System.Text.UTF8Encoding.UTF8.GetString(Buffer, Pos, len);
            Pos += len;

            return v;
        }
        public bool IsEnd()
        {
            return (Pos == Buffer.Length);
        }
        public int GetPos()
        {
            return Pos;
        }
    }

    public class ByteWriter
    {
        List<byte> Buffer = new List<byte>(1024);

        public void Write(short v)
        {
            Buffer.AddRange(BitConverter.GetBytes(v));
        }
        public void Write(int v)
        {
            Buffer.AddRange(BitConverter.GetBytes(v));
        }
        public void Write(float v)
        {
            Buffer.AddRange(BitConverter.GetBytes(v));
        }
        public void Write(byte[] array)
        {
            Buffer.AddRange(array);
        }
        public void Write(string v)
        {
            byte[] bytes = System.Text.UTF8Encoding.UTF8.GetBytes(v);
            Write(bytes.Length);
            Buffer.AddRange(bytes);
        }

        public void Reset()
        {
            Buffer.Clear();
        }
        public byte[] GetBytes()
        {
            return Buffer.ToArray();
        }
    }
}
