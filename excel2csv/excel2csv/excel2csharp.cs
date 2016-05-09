/****************************************************************************
Copyright (c) 2013-2025,大连-游你酷伴.
 This is not a free-ware .DO NOT use it without any authorization.
 * 坚持做有意思的游戏
 * 
 * date : 5/9/2016 10:17:00 PM
 * author : Labor
 * purpose : 
****************************************************************************/
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace SHGame
{
    public class excel2csharp
    {
        public void DoAllAction()
        {
            string rootPath = "../../res";
            List<XMLStruct.Excel> objs = xml2class.Read<XMLStruct.Excel>(rootPath + "/model_template_gen.xml");

            foreach (var excel in objs)
            {
                DoAction(excel.FileName);
            }
        }
        public void DoAction(string filename)
        {
            string rootPath = "../../res";
            List<XMLStruct.Excel> objs = xml2class.Read<XMLStruct.Excel>(rootPath + "/model_template_gen.xml");

            XMLStruct.Excel excel = objs.Find(x => x.FileName.Equals(filename, StringComparison.CurrentCultureIgnoreCase));
            if (excel == null)
            {
                Console.WriteLine(string.Format("无法找到{0}的xml文件：", filename));
                return;
            }
            bool exist = File.Exists(string.Format(rootPath + "/excel/{0}.xls", filename))
                    || File.Exists(string.Format(rootPath + "/excel/{0}.xlsx", filename));
            if (!exist)
            {
                Console.WriteLine("无法找到excel文件：" + filename);
                return;
            }

            foreach (var sheet in excel.sheets)
            {
                List<XMLStruct.Column> cols = xml2class.Read<XMLStruct.Column>(string.Format(rootPath + "/model/{0}.xml", sheet.Template));

                BaseType bt = new BaseType();
                bt.TypeName = sheet.Template;
                bt.Members = new List<MemberType>();
                bt.Comment = string.Format("{0}_{1}", filename, bt.TypeName);

                foreach (var col in cols)
                {
                    if (!string.IsNullOrEmpty(col.Ignore))
                    {
                        continue;
                    }

                    MemberType mt = new MemberType();
                    mt.TypeName = col.ValueType;
                    mt.MemberName = GenerateCode.ToMarked(col.ValueName);
                    mt.IsBaseType = true;// GenerateCode.IsBaseType(mt.TypeName);   // csv只允许使用基础类型，所以写死拉
                    mt.Comment = col.Comment;
                    bt.Members.Add(mt);
                }

                GenerateCode.GenerateFile(bt, rootPath + "/template/TemplateCSharp.txt", rootPath + "/csharp/");
            }
        }
    }
}
