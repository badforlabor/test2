using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.IO;
using NVelocity;
using NVelocity.App;
using NVelocity.Runtime;
using NVelocity.Context;

namespace AutoGenProtocol
{
    class MemberType
    {
        public string Type;
        public string MemberName;
        public string Comment;
    }
    class BaseType
    {
        public string Type;
        public string Comment;
        public List<MemberType> Members;
    }
    class Program
    {
        static string BaseDir = "auto_gen/";
        static void Main(string[] args)
        {
            XmlDocument xml = new XmlDocument();
            xml.Load("D:\\liubo\\github2\\test2\\AutoGenProtocol\\test.xml");
            
            XmlNode env = xml.SelectSingleNode("/root/env");
            if (env != null)
            {
                Console.WriteLine("env-log = " + env.Attributes["log"].Value);
            }

            // 基础数据类型
            XmlNodeList baseTypes = xml.SelectNodes("/root/baseType");
            for (int i = 0; i < baseTypes.Count; i++)
            {
                GenerateBaseType(baseTypes[i]);
            }

            // 协议
            XmlNodeList protocols = xml.SelectNodes("/root/protocol");
            for (int i = 0; i < protocols.Count; i++)
            {
                GenerateProtocol(protocols[i]);   
            }
            Console.WriteLine("root-name = " + xml.DocumentElement.Name);
        }
        static void GenerateBaseType(XmlNode baseType)
        {
            GenerateClassFile(baseType, false);
        }
        static void GenerateProtocol(XmlNode protocol)
        {
            GenerateClassFile(protocol, true);
        }
        static void GenerateClassFile(XmlNode classXmlData, bool protocol)
        {
            bool iscg = false;
            Console.WriteLine("生成协议，协议的名字是：" + classXmlData.Attributes["name"]);
            XmlNodeList members = classXmlData.SelectNodes("member");
            for (int i = 0; i < members.Count; i++)
            {
                Console.WriteLine("   协议体：" + members[i].Attributes["type"].Value + ", " + members[i].Attributes["name"].Value);
            }

            // 协议存放路径
            string path = classXmlData.Attributes["folder"] == null ? "" : classXmlData.Attributes["folder"].Value;
            path = BaseDir + path + "\\";
            Directory.CreateDirectory(path);

            string protocolName = classXmlData.Attributes["name"].Value;
            string MarkedName = "";
            // 把形如“cg_activity”这种格式转化成“CGActivity”
            if (protocolName.StartsWith("cg_"))
            {
                iscg = true;
                MarkedName = "CG" + ToMarked(protocolName.Substring(4));
            }
            else if (protocolName.StartsWith("gc_"))
            {
                MarkedName = "GC" + ToMarked(protocolName.Substring(4));
            }
            else
            {
                if (protocol)
                {
                    throw new Exception("协议必须以 cg_ 或者 gc_ 开头");
                }
                else
                {
                    MarkedName = ToMarked(protocolName);
                }
            }
            Console.WriteLine("     class's MarkedName=" + MarkedName);
            
            // 协议注释
            string comment = classXmlData.Attributes["comment"] == null ? "" : classXmlData.Attributes["comment"].Value;

            BaseType baseType = new BaseType();
            baseType.Comment = comment;
            baseType.Type = MarkedName;
            baseType.Members = new List<MemberType>();
            for (int i = 0; i < classXmlData.ChildNodes.Count; i++)
            {
                XmlNode node = classXmlData.ChildNodes[i];
                MemberType mt = new MemberType();
                mt.Comment = node.Attributes["comment"] == null ? "" : node.Attributes["comment"].Value;
                mt.Type = node.Attributes["type"].Value;
                mt.MemberName = ToMarked(node.Attributes["name"].Value);
                baseType.Members.Add(mt);
            }

            GenerateFile(baseType, "BaseProtocol.txt");
#if false

            StringBuilder builder = new StringBuilder();
            builder.Append("\n");
            builder.Append("/****************************************************************************\n");
            builder.Append(" */t该文件是自动生成的，不需要手动修改！\n");
            builder.Append(" *\n");
            builder.Append(" * data: " + DateTime.Now.ToShortDateString());
            builder.Append(" * author: auto-gen-tools");
            builder.Append(" * purpose: " + comment);
            builder.Append("****************************************************************************/\n");
            builder.Append("\n");

            if (protocol)
            {
                builder.Append("using System;\n");
                builder.Append("using System.Collections.Generic;\n");
                builder.Append("using System.ComponentModel;\n");

                builder.Append("\n");
                builder.Append("namespace SHGame\n");
                builder.Append("{\n");
                builder.Append(string.Format("\t[MLMessageProvider(MLMessageType{0}.Data)]\n", MarkedName));
                builder.Append(string.Format("\tclass {0} : MLCGMessage\n", MarkedName));
            }
            else
            {
                builder.Append("\tclass " + MarkedName + "\n");
            }
            builder.Append("\t{\n\n");
#endif
        }

        static void GenerateFile(BaseType bt, string fileName)
        {
            string file = "D:\\liubo\\github2\\test2\\AutoGenProtocol\\AutoGenProtocol\\" + fileName;

            VelocityEngine vltEngine = new VelocityEngine();
            vltEngine.SetProperty(RuntimeConstants_Fields.RESOURCE_LOADER, "file");
            vltEngine.SetProperty(RuntimeConstants_Fields.FILE_RESOURCE_LOADER_PATH, "D:\\liubo\\github2\\test2\\AutoGenProtocol\\AutoGenProtocol\\");
            vltEngine.Init();

            VelocityContext context1 = new VelocityContext();
            context1.Put("BaseType", bt);

            if (File.Exists(file))
            {
                Console.WriteLine("存在模板文件！");
            }

            Template template = vltEngine.GetTemplate(fileName);

            StringWriter autoScriptWriter = new StringWriter();
            template.Merge(context1, autoScriptWriter);

            File.WriteAllText(@"D:\liubo\github2\test2\AutoGenProtocol\AutoGenProtocol\auto-gen\" + bt.Type + ".cs", autoScriptWriter.GetStringBuilder().ToString());
        }




        static string ToMarked(string str)
        {
            string[] str_array = str.Split('_');
            string ret = "";
            for (int i = 0; i < str_array.Length; i++)
            {
                ret += ToUpperFirst(str_array[i]);
            }
            return ret;
        }
        static string ToUpperFirst(string str)
        {
            char[] str_array = str.ToCharArray();
            if (str_array[0] >= 'a' && str_array[0] <= 'z')
            {
                str_array[0] = (char)(str_array[0] + 'A' - 'a');
            }
            return new string(str_array);
        }
    }
}
