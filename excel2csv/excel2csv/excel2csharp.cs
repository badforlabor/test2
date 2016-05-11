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
using NPOI.HSSF.UserModel;
using System.Text;

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

            string shortname = filename.Replace('\\', '_').Replace('/', '_');

            FileStream fs = File.Open(rootPath + "/excel/" + filename + ".xls", FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            HSSFWorkbook wk = new HSSFWorkbook(fs);

            for (int i = 0; i < excel.sheets.Length; i++)
            {
                if (i >= wk.NumberOfSheets)
                {
                    Console.WriteLine("无法找到sheet, idx=" + i);
                    continue;
                }

                HSSFSheet sheet = wk.GetSheetAt(i) as HSSFSheet;
                StringBuilder csvBuffer = new StringBuilder();
                string sheetName = sheet.SheetName;

                var sheetModel = excel.sheets[i];
                List<XMLStruct.Column> cols = xml2class.Read<XMLStruct.Column>(string.Format(rootPath + "/model/{0}.xml", sheetModel.Template));

                bool header = false;

                for (int j = sheet.FirstRowNum; j < sheet.LastRowNum; j++)
                {
                    HSSFRow row = sheet.GetRow(j) as HSSFRow;
                    
                    // 以"#"开头的表示当前行为注释
                    if (row.FirstCellNum < 0
                            || string.IsNullOrEmpty((row.GetCell(row.FirstCellNum) as HSSFCell).ToString().Trim())
                            || (row.GetCell(row.FirstCellNum) as HSSFCell).ToString().Trim().StartsWith("#"))
                    {
                        if (header)
                        {
                            throw new Exception("该行的第一个单元格格式错误！");
                        }
                        continue;
                    }                    
                    
                    for (int k = row.FirstCellNum; k < row.LastCellNum; k++)
                    {
                        if (k > 0 && k-1 < cols.Count && string.IsNullOrEmpty(cols[k - 1].Ignore))
                        {                            
                        }
                        else
                        {
                            continue;
                        }
                        
                        HSSFCell cell = row.GetCell(k) as HSSFCell;

                        if (header)
                        {
                            // 表头
                            csvBuffer.Append(cell.ToString().Trim());
                        }
                        else
                        { 
                            // 
                            csvBuffer.Append(cell.ToString().Trim());
                        }
                        if (k != row.LastCellNum - 1)
                        {
                            csvBuffer.Append(",");
                        }
                    }
                    // 换行
                    csvBuffer.Append("\n");

                    // 处理完表头了
                    header = true;
                }

                // 导出到csv文件
                File.WriteAllText(rootPath + "/csv/" + shortname + "_" + sheetName, csvBuffer.ToString());

                
                // 接下来生成csharp代码
                BaseType bt = new BaseType();
                bt.TypeName = sheetModel.Template;
                bt.Members = new List<MemberType>();
                bt.Comment = string.Format("{0}_{1}", shortname, sheetName);

                foreach (var col in cols)
                {
                    if (!string.IsNullOrEmpty(col.Ignore))
                    {
                        continue;
                    }

                    CsvMemberType mt = new CsvMemberType();
                    mt.TypeName = col.ValueType;
                    mt.MemberName = GenerateCode.ToMarked(col.ValueName);
                    mt.IsBaseType = true;// GenerateCode.IsBaseType(mt.TypeName);   // csv只允许使用基础类型，所以写死拉
                    mt.Comment = col.Comment;
                    mt.Denominator = col.Denominator;
                    bt.Members.Add(mt);
                }
                GenerateCode.GenerateFile(bt, rootPath + "/template/TemplateCSharp.txt", rootPath + "/csharp/");
            }
        }
    }
}
