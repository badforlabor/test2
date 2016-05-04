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
    public class A
    {
        public string comment { get; set; }
    }
    public class MemberType
    {
        public string TypeName { get; set; }
        public string MemberName { get; set; }
        public string Comment { get; set; }
        public bool IsArray { get; set; }
        // int，bool，string等才算是基础类型
        public bool IsBaseType { get; set; }
    }
    public class BaseType
    {
        public string TypeName { get; set; }
        public string Comment { get; set; }
        public List<MemberType> Members { get; set; }
    }
    public class ClassID
    {
        public string Name { get; set; }
        public string Value { get; set; }
    }
    class Program
    {
        static string BaseDir = "auto_gen/";
        static void Main(string[] args)
        {
            XmlDocument xml = new XmlDocument();
            xml.Load("D:\\liubo\\github2\\test2\\AutoGenProtocol\\protocols.xml");
            
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


            // 生成message-id
            List<string> protocolNames = new List<string>();
            for (int i = 0; i < protocols.Count; i++)
            {
                protocolNames.Add(protocols[i].Attributes["name"].Value);
            }
            GenerateMessageType(protocolNames);
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
            int templateid = 0;
            string[] templateFiles = { 
                                         "cg_protocol_template.txt",
                                         "gc_protocol_template.txt",
                                         "base_type_template.txt",
                                     };

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
                templateid = 0;
                MarkedName = "CG" + ToMarked(protocolName.Substring(3));
            }
            else if (protocolName.StartsWith("gc_"))
            {
                templateid = 1;
                MarkedName = "GC" + ToMarked(protocolName.Substring(3));
            }
            else
            {
                if (protocol)
                {
                    throw new Exception("协议必须以 cg_ 或者 gc_ 开头");
                }
                else
                {
                    templateid = 2;
                    MarkedName = ToMarked(protocolName);
                }
            }
            Console.WriteLine("     class's MarkedName=" + MarkedName);
            
            // 协议注释
            string comment = classXmlData.Attributes["comment"] == null ? "" : classXmlData.Attributes["comment"].Value;
            string folder = classXmlData.Attributes["folder"] == null ? "" : classXmlData.Attributes["folder"].Value;

            if (!string.IsNullOrEmpty(folder) && !folder.EndsWith("\\"))
            {
                folder += "\\";
            }

            BaseType baseType = new BaseType();
            baseType.Comment = comment;
            baseType.TypeName = MarkedName;
            baseType.Members = new List<MemberType>();
            for (int i = 0; i < classXmlData.ChildNodes.Count; i++)
            {
                XmlNode node = classXmlData.ChildNodes[i];
                MemberType mt = new MemberType();
                mt.Comment = node.Attributes["comment"] == null ? "" : node.Attributes["comment"].Value;
                mt.TypeName = node.Attributes["type"].Value;
                mt.MemberName = ToMarked(node.Attributes["name"].Value);
                mt.IsArray = (node.Attributes["array"] == null) ? false : node.Attributes["array"].Value.Equals("1");
                mt.IsBaseType = (mt.TypeName == "bool" || mt.TypeName == "int" || mt.TypeName.ToLower() == "string");
                baseType.Members.Add(mt);
            }

            GenerateFile(baseType, templateFiles[templateid], folder);
        }
        static void GenerateMessageType(List<string> protocolNames)
        {
            XmlDocument xmlIds = new XmlDocument();
            xmlIds.Load("D:\\liubo\\github2\\test2\\AutoGenProtocol\\message-id.xml");
            XmlNodeList ids = xmlIds.SelectNodes("/root/id");
            List<ClassID> classIDs = new List<ClassID>();
            for (int i = 0; i < ids.Count; i++)
            {
                classIDs.Add(new ClassID() { Name = ids[i].Attributes["name"].Value, Value = ids[i].Attributes["value"].Value.Trim() });
            }

            // 生成“唯一值”的函数
            int ret = 1;
            System.Func<int> GetId = delegate()
            {
                while (ret < 65535)
                {
                    if (classIDs.Find(x => x.Value.Equals(ret.ToString())) == null)
                    {
                        break;
                    }
                    ret++;
                }
                return ret;
            };

            // 插入新加的，保证protocol.xml和message_id.xml的顺序是一样的
            int lastIdx = -1;
            foreach (var pname in protocolNames)
            {
                int it = classIDs.FindIndex(x => x.Name == pname);
                if (it == -1)
                {
                    // 新的，
                    classIDs.Insert(lastIdx == -1 ? classIDs.Count : lastIdx, new ClassID() { Name = pname, Value = GetId().ToString() });
                    lastIdx = classIDs.Count;
                }
                else
                {
                    lastIdx = it;
                }
            }
            // 删掉无效的
            for (int i = 0; i < classIDs.Count; i++)
            {
                if (protocolNames.Find(x => x == classIDs[i].Name) == null)
                {
                    classIDs.RemoveAt(i);
                    i--;
                }
            }

            // 生成.cs文件
            GenerateClassIDs(classIDs);

            // 生成.xml文件
            XmlNode idRoot = xmlIds.SelectSingleNode("/root");
            idRoot.RemoveAll();
            for (int i = 0; i < classIDs.Count; i++)
            {
                XmlElement ele = xmlIds.CreateElement("id");
                ele.SetAttribute("name", classIDs[i].Name);
                ele.SetAttribute("value", classIDs[i].Value);
                idRoot.AppendChild(ele);
            }
            xmlIds.Save("D:\\liubo\\github2\\test2\\AutoGenProtocol\\message-id.xml");
        }
        static void GenerateClassIDs(List<ClassID> classIDs)
        {
            BaseType bt = new BaseType();
            bt.TypeName = "MessageType";
            bt.Comment = "定义协议ID";
            bt.Members = new List<MemberType>();
            foreach (var it in classIDs)
            {
                string typeName = it.Name;
                if (typeName.StartsWith("cg_"))
                {
                    typeName = "CG_" + typeName.Substring(3);
                }
                else if (typeName.StartsWith("gc_"))
                {
                    typeName = "GC_" + typeName.Substring(3);
                }
                bt.Members.Add(new MemberType() { TypeName = ToMarked(typeName), MemberName = it.Value });
            }
            GenerateFile(bt, "message_id_template.txt", "");
        }


        static void GenerateFile(BaseType bt, string fileName, string folder)
        {
            string templateFile = "D:\\liubo\\github2\\test2\\AutoGenProtocol\\AutoGenProtocol\\" + fileName;
            string outputDir = @"D:\liubo\github2\test2\AutoGenProtocol\AutoGenProtocol\auto-gen\" + folder;
            string outputFile = outputDir + bt.TypeName + ".cs";
            Directory.CreateDirectory(outputDir);

            if (!File.Exists(templateFile))
            {
                throw new Exception("无法找到模板文件：" + templateFile);
            }

            string buff = GetNVelocityBuffer(bt, fileName);

            bool export = true;
            if (File.Exists(outputFile))
            {
                string fileContent = File.ReadAllText(outputFile);
                if (fileContent == buff)
                {
                    export = false;
                }
            }

            if (export)
            {
                File.WriteAllText(outputFile, buff);
            }
        }
        static string GetNVelocityBuffer(BaseType bt, string fileName)
        {
            VelocityEngine vltEngine = new VelocityEngine();
            vltEngine.SetProperty(RuntimeConstants_Fields.RESOURCE_LOADER, "file");
            vltEngine.SetProperty(RuntimeConstants_Fields.FILE_RESOURCE_LOADER_PATH, "D:\\liubo\\github2\\test2\\AutoGenProtocol\\AutoGenProtocol\\");
            vltEngine.Init();

            VelocityContext context1 = new VelocityContext();
            context1.Put("BaseType", bt);
            context1.Put("DateTime", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));

            Template template = vltEngine.GetTemplate(fileName);

            StringWriter autoScriptWriter = new StringWriter();
            template.Merge(context1, autoScriptWriter);

            return autoScriptWriter.GetStringBuilder().ToString();
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
