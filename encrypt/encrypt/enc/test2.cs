/****************************************************************************
Copyright (c) 2013-2014,Dalian-LingYOU tech.
 This is not a free-ware .DO NOT use it without any authorization.
 * 
 * date : 2016/4/21 11:53:58
 * author : Labor
 * purpose : 
****************************************************************************/
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;

namespace MLGame
{
    public class test2
    {
        static void Main(string[] args)
        {
#if TEST
            CombineAll();
            UnCombineAll();
#else
            if (args.Length > 1)
            {
                Console.WriteLine("执行：" + args[0]);
                if (args[0] == "-e")
                {
                    CombineAll(args[1]);
                }
                else if (args[0] == "-d")
                {
                    UnCombineAll(args[1]);
                }
            }
            else
            {
                Console.WriteLine("格式不正确，使用这种格式，比如加密D盘aaa文件夹中的内容，那么命令为： enc -e d:\aaa");
            }
#endif
        }
        public static void CombineAll()
        {
            string dir = "D:\\123\\gzip\\combine";
            string outFilename = "D:\\123\\gzip\\123.combine";

            CombineAll(dir, outFilename);
        }
        public static void CombineAll(string dir)
        {
            CombineAll(dir, dir + ".bin");
        }
        public static void CombineAll(string dir, string outFilename)
        {
            dir = dir.Replace('/', '\\');
            outFilename = outFilename.Replace('/', '\\');
            Console.WriteLine("加密“" + dir + "” 到 “" + outFilename + "”");
            if (!Directory.Exists(dir))
            {
                Console.WriteLine("无法找到该目录：" + dir);
                return;
            }
            FileStream outputStream = File.OpenWrite(outFilename);

            /**
             * 文件结构
             *      第0个字节，1表示文件，2表示目录
             *      接下来，记录文件名字
             *      接来下，如果是文件：
             *          顺延的第1~4个字节，表示文件大小
             *          顺延的第5~N个字节，表示文件内容
             * ***/
            Combine(dir, dir.Contains("\\") ? dir.Substring(0, dir.LastIndexOf('\\')) : "", outputStream);
            outputStream.WriteByte(3);

            outputStream.Flush();
            outputStream.Close();
        }
        public static void UnCombineAll()
        {
            string inFilename = "D:\\123\\gzip\\123.combine";
            string dir = "D:\\123\\gzip\\uncombine";
            UnCombineAll(inFilename, dir);
        }
        public static void UnCombineAll(string inFilename)
        {
            string shortName = inFilename.Contains(".") ? inFilename.Substring(0, inFilename.LastIndexOf(".")) : inFilename;
            UnCombineAll(inFilename, shortName + "_uncombined");
        }
        public static void UnCombineAll(string inFilename, string dir)
        {
            inFilename = inFilename.Replace('/', '\\');
            dir = dir.Replace('/', '\\');
            Console.WriteLine("解密“" + inFilename + "” 到 “" + dir + "”");
            FileStream inputStream = File.OpenRead(inFilename);
            UnCombine(inputStream, dir);
            inFilename.Clone();
        }
        static void Combine(string fileName, string baseDir, Stream outputStream)
        {
            Console.WriteLine("整合：" + fileName);


            bool bdir = Directory.Exists(fileName);
            if (bdir)
            {
                outputStream.WriteByte(2);
            }
            else
            {
                outputStream.WriteByte(1);
            }

            string shortName = fileName.Substring(baseDir.Length == 0 ? 0 : baseDir.Length + 1);
            byte[] bytes = System.Text.Encoding.UTF8.GetBytes(shortName);
            byte[] len = BitConverter.GetBytes(bytes.Length);
            outputStream.Write(len, 0, len.Length);
            outputStream.Write(bytes, 0, bytes.Length);

            if(bdir)
            {
                string[] files = Directory.GetFiles(fileName);
                foreach(var f in files)
                {
                    Combine(f, baseDir, outputStream);
                }

                string[] dirs = Directory.GetDirectories(fileName);
                foreach(var d in dirs)
                {
                    Combine(d, baseDir, outputStream);
                }
            }
            else
            {
                FileStream fs = File.OpenRead(fileName);
                len = BitConverter.GetBytes((int)fs.Length);
                outputStream.Write(len, 0, len.Length);

                byte[] buffer = new byte[1024];
                int read_cnt = 0;
                do
                {
                    read_cnt = fs.Read(buffer, 0, buffer.Length);
                    if(read_cnt > 0)
                    {
                        outputStream.Write(buffer, 0, read_cnt);
                    }
                }
                while(read_cnt == buffer.Length);

                fs.Close();
            }
        }
        static void UnCombine(Stream inputStream, string baseDirName)
        {
            int v = inputStream.ReadByte();
            if (v == 1)
            { 
            }
            else if (v == 2)
            {

            }
            else if (v == -1 || v == 3)
            {
                // 结束了。
                return;
            }
            else
            {
                throw new Exception("无法识别的标志:" + v);
            }

            // 文件
            byte[] buffer = new byte[1024];
            int buffer_len = 0;
            buffer_len = inputStream.Read(buffer, 0, sizeof(int));
            int name_len = BitConverter.ToInt32(buffer, 0);
            buffer_len = inputStream.Read(buffer, 0, name_len);
            string shortName = System.Text.Encoding.UTF8.GetString(buffer, 0, name_len);
            string filename = baseDirName + "\\" + shortName;

            Console.WriteLine("解析到：" + filename);

            if (v == 1)
            {
                FileStream fs = File.Create(filename);

                inputStream.Read(buffer, 0, 4);
                int file_len = BitConverter.ToInt32(buffer, 0);

                do
                {
                    buffer_len = file_len > 1024 ? 1024 : file_len;
                    buffer_len = inputStream.Read(buffer, 0, buffer_len);
                    if (buffer_len > 0)
                    {
                        fs.Write(buffer, 0, buffer_len);
                    }
                    file_len -= buffer_len;
                }
                while (buffer_len == buffer.Length && file_len > 0);
                fs.Close();

                // 读取下一个
                UnCombine(inputStream, baseDirName);
            }
            else
            {
                Directory.CreateDirectory(filename);

                // 进入文件夹内读取
                UnCombine(inputStream, baseDirName);
            }
        }
        public static void TestGzip()
        {
            string inFilename = "D:\\123\\gzip\\123.txt";
            string outFilename = "D:\\123\\gzip\\123.txt.gzip";
            FileStream outFile = File.OpenWrite(outFilename);
            FileStream inFile = File.OpenRead(inFilename);
            GZipStream gzip = new GZipStream(outFile, CompressionLevel.Fastest);

            byte[] buffer = new byte[1024];
            int len = 0;
            do
            {
                len = inFile.Read(buffer, 0, buffer.Length);
                if (len > 0)
                {
                    gzip.Write(buffer, 0, len);
                }
            }
            while(len == buffer.Length);

            gzip.Flush();
            gzip.Close();
            inFile.Close();
        }
        public static void UnGZipFile()
        {
            string inFilename = "D:\\123\\gzip\\123.txt.gzip";
            string outFilename = "D:\\123\\gzip\\123-unzip.txt";
            FileStream inFile = File.OpenRead(inFilename);
            FileStream outFile = File.OpenWrite(outFilename);
            GZipStream gzip = new GZipStream(inFile, CompressionMode.Decompress);

            byte[] buffer = new byte[1024];
            int len = 0;
            do
            {
                len = gzip.Read(buffer, 0, buffer.Length);
                if (len > 0)
                {
                    outFile.Write(buffer, 0, len);
                }
            }
            while (len == buffer.Length);
            outFile.Flush();
            outFile.Close();
            gzip.Close();
        }
    }
}
