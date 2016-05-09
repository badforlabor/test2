/****************************************************************************
Copyright (c) 2013-2025,大连-游你酷伴.
 This is not a free-ware .DO NOT use it without any authorization.
 * 坚持做有意思的游戏
 * 
 * date : 5/7/2016 6:04:45 AM
 * author : Labor
 * purpose : 
****************************************************************************/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.IO;
using System.Reflection;

namespace SHGame
{
    public static class XmlAttributeCollectionExtension
    {
        public static XmlAttribute GetAttribute(this XmlAttributeCollection self, string key)
        {
            foreach (var it in self)
            {
                XmlAttribute attr = it as XmlAttribute;
                if (attr.Name.Equals(key, StringComparison.CurrentCultureIgnoreCase))
                {
                    return attr;
                }
            }
            return null;
        }
    }
    public static class xml2class
    {
        public static List<T> Read<T>(string path)
        {
            List<object> objs = Read(path, typeof(T));

            List<T> ret = new List<T>();
            ret.AddRange(objs.Cast<T>());
            return ret;
        }
        public static List<object> Read(string path, Type classType)
        {
            return Read(path, classType, "/root/" + classType.Name.ToLower());
        }
        public static List<object> Read(string path, Type classType, string tag)
        {
#if false
            // 读取文件内容，并且转化成小写格式，不过这样会导致xml中的value值也变成小写，所以放弃这种方式
            string stringbuffer = File.ReadAllText(path);
            stringbuffer = stringbuffer.ToLower();
            byte[] string_array = Encoding.UTF8.GetBytes(stringbuffer);
            MemoryStream stream = new MemoryStream(string_array);             //convert stream 2 string      
            StreamReader reader = new StreamReader(stream);

            XmlDocument xml = new XmlDocument();
            xml.Load(reader);
#else
            XmlDocument xml = new XmlDocument();
            xml.Load(path);
#endif
            // 解析“/root/childtype”获得root节点和childtype字符串
            List<string> str_arr = split_string.SplitString<string>(tag, "/");
            XmlNode baseroot = xml;
            for (int i = 0; i < str_arr.Count - 1; i++)
            {
                if (string.IsNullOrEmpty(str_arr[i]))
                    continue;

                baseroot = baseroot.SelectSingleNode(str_arr[i]);
            }
            
            return ProcessNode(baseroot, str_arr[str_arr.Count - 1], classType);
        }
        static List<object> ProcessNode(XmlNode root, string direct_path, Type classType)
        {
            List<object> ret = new List<object>();
            XmlNodeList baseTypes = root.SelectNodes(direct_path);
            for (int i = 0; i < baseTypes.Count; i++)
            {
                XmlNode xmldata = baseTypes[i];
                object obj = Activator.CreateInstance(classType);
                // 读取该类的成员变量和属性：只获取public的
                var fields = classType.GetFields(BindingFlags.Instance | BindingFlags.Public);
                var properties = classType.GetProperties(BindingFlags.Instance | BindingFlags.Public);

                foreach (var field in fields)
                {
                    if (field.FieldType.IsArray)
                    {
                        Type embedtype = field.FieldType.GetElementType();
                        List<object> child_objs = ProcessNode(xmldata, embedtype.Name.ToLower(), embedtype);
                        Array value_array = Array.CreateInstance(embedtype, child_objs.Count);
                        for(int k=0; k<child_objs.Count; k++)
                        {
                            value_array.SetValue(child_objs[k], k);
                        }
                        field.SetValue(obj, value_array);
                        continue;
                    }

                    string memberName = field.Name.ToLower();
                    // 如果是只读属性，那么过滤掉！
                    if (xmldata.Attributes.GetAttribute(memberName) != null)
                    {
                        field.SetValue(obj, Convert.ChangeType(xmldata.Attributes.GetAttribute(memberName).Value, field.FieldType));
                    }
                }

                foreach (var prop in properties)
                {
                    if (prop.PropertyType.IsArray)
                    {
                        Type embedtype = prop.PropertyType.GetElementType();
                        List<object> child_objs = ProcessNode(xmldata, embedtype.Name.ToLower(), embedtype);
                        Array value_array = Array.CreateInstance(embedtype, child_objs.Count);
                        for (int k = 0; k < child_objs.Count; k++)
                        {
                            value_array.SetValue(child_objs[k], k);
                        }
                        prop.SetValue(obj, value_array);
                        continue;
                    }

                    string memberName = prop.Name.ToLower();
                    // 如果是只读属性，那么过滤掉！
                    if (xmldata.Attributes.GetAttribute(memberName) != null)
                    {
                        prop.SetValue(obj, Convert.ChangeType(xmldata.Attributes.GetAttribute(memberName).Value, prop.PropertyType));
                    }
                }

                ret.Add(obj);
            }

            return ret;
        }
    }
    public static class excel2classTest
    {
        class SheetPro
        {
            public string value;
        }
        class Sheet
        {
            public string template;
            public SheetPro[] pros;
        }
        class Excel
        {
            public string name { get; set; }
            public string comment;
            public int v1;
            public float v2;

            //public Sheet[] sheets;
            public Sheet[] sheets { get; set; }
        };
        public static void Test()
        {
            List<Excel> objs = xml2class.Read<Excel>("../../res/test_case1.xml");
        }
    }
}
