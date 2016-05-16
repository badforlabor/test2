/****************************************************************************
Copyright (c) 2013-2025,大连-游你酷伴.
 This is not a free-ware .DO NOT use it without any authorization.
 * 坚持做有意思的游戏
 * 
 * date : 5/9/2016 10:19:20 PM
 * author : Labor
 * purpose : 
****************************************************************************/
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

namespace SHGame
{
    public class MemberType
    {
        public string TypeName { get; set; }
        public string MemberName { get; set; }
        public string Comment { get; set; }
        public bool IsArray { get; set; }
        // int，bool，string等才算是基础类型
        public bool IsBaseType { get; set; }
    }
    public class CsvMemberType : MemberType
    {
        public int Denominator { get; set; }
    }
    public class BaseType
    {
        public string TypeName { get; set; }
        public string Comment { get; set; }
        public List<MemberType> Members { get; set; }
    }
    public static class GenerateCode
    {
        public static bool IsBaseType(string vt)
        {
            return (vt == "bool" || vt == "int" || vt.ToLower() == "string");
        }
        public static bool IsStringType(string vt)
        {
            return vt.ToLower().Equals("string");
        }
        public static string ToMarked(string str)
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
        public static void WriteAllText(string filename, string buffer)
        {
            if (File.Exists(filename))
            {
                if (File.ReadAllText(filename) == buffer)
                {
                    return;
                }
            }
            File.WriteAllText(filename, buffer);
        }
        public static void GenerateFile(BaseType bt, string templateFile, string outputDir)
        {
//             string templateFile = "D:\\liubo\\github2\\test2\\AutoGenProtocol\\AutoGenProtocol\\" + fileName;
//             string outputDir = @"D:\liubo\github2\test2\AutoGenProtocol\AutoGenProtocol\auto-gen\" + folder;
            string outputFile = outputDir + bt.TypeName + ".cs";
            Directory.CreateDirectory(outputDir);

            if (!File.Exists(templateFile))
            {
                throw new Exception("无法找到模板文件：" + templateFile);
            }

            string buff = GetNVelocityBuffer(bt, templateFile);

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
        static string GetNVelocityBuffer(BaseType bt, string templateFile)
        {
            templateFile = templateFile.Replace('/', '\\');
            string dirTemplate = templateFile.Substring(0, templateFile.LastIndexOf("\\"));
            string fileName = templateFile.Substring(dirTemplate.Length+1);

            VelocityEngine vltEngine = new VelocityEngine();
            vltEngine.SetProperty(RuntimeConstants_Fields.RESOURCE_LOADER, "file");
            vltEngine.SetProperty(RuntimeConstants_Fields.FILE_RESOURCE_LOADER_PATH, dirTemplate);
            vltEngine.Init();

            VelocityContext context1 = new VelocityContext();
            context1.Put("BaseType", bt);
            context1.Put("DateTime", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));

            Template template = vltEngine.GetTemplate(fileName);

            StringWriter autoScriptWriter = new StringWriter();
            template.Merge(context1, autoScriptWriter);

            return autoScriptWriter.GetStringBuilder().ToString();
        }

    }
}
