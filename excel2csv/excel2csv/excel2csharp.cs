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

                // 设置csv的表头
                csvBuffer.Append("id");
                for (int j = 0; j < cols.Count; j++)
                {
                    if (!string.IsNullOrEmpty(cols[j].Ignore))
                    {
                        continue;
                    }
                    csvBuffer.Append(",");
                    csvBuffer.Append(cols[j].ValueName);
                }
                csvBuffer.Append("\n");

                // 设置具体内容
                for (int j = 0; j < sheet.LastRowNum - sheet.FirstRowNum; j++)
                {
                    HSSFRow row = sheet.GetRow(j) as HSSFRow;

                    HSSFCell cell0 = row == null ? null : (row.GetCell(0) as HSSFCell);
                    string cell0Value = cell0 == null ? "" : cell0.ToString().Trim();
                    // 以"#"开头的表示当前行为注释
                    if (string.IsNullOrEmpty(cell0Value) || cell0Value.StartsWith("#"))
                    {
                        continue;
                    }                    
                    
                    // 第0个元素一定是个id，int类型
                    csvBuffer.Append(row.GetCell(0).NumericCellValue.ToString());
                    for (int k = 0; k < cols.Count; k++)
                    {
                        // 忽略列属性为Ignore的内容
                        if (!string.IsNullOrEmpty(cols[k].Ignore))
                        {
                            continue;                          
                        }
                        
                        // cols不包含第一列：id列
                        HSSFCell cell = row.GetCell(k + 1) as HSSFCell;

                        csvBuffer.Append(",");
                        string cellvalue = GetCellValue(cell);

                        // 非法检查
                        if (cellvalue.IndexOfAny(new char[] { '\r', '\n' }) != -1)
                        {
                            throw new Exception(string.Format("{0}_{1}表格含有非法字段：row={2}, col={3}", shortname, sheetName, j, k+1));
                        }
                        else if (cellvalue.IndexOfAny(new char[] { ' ', ',', '\t' }) != -1)
                        {
                            cellvalue = string.Format("\"{0}\"", cellvalue);
                        }

                        csvBuffer.Append(cellvalue);
                    }
                    // 换行
                    csvBuffer.Append("\n");
                }

                // 导出到csv文件
                GenerateCode.WriteAllText(rootPath + "/csv/" + shortname + "_" + sheetName + ".csv", csvBuffer.ToString());
                
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

        static string GetCellValue(HSSFCell cell)
        {
            if (cell == null)
            {
                return "";
            }

            switch (cell.CellType)
            {
                case NPOI.SS.UserModel.CellType.Unknown:
                    return "";
                case NPOI.SS.UserModel.CellType.Numeric:
                    return cell.NumericCellValue.ToString();
                case NPOI.SS.UserModel.CellType.String:
                    return cell.StringCellValue;
                case NPOI.SS.UserModel.CellType.Formula:
                    if (cell.CachedFormulaResultType == NPOI.SS.UserModel.CellType.Numeric)
                    {
                        return cell.NumericCellValue.ToString();
                    }
                    else
                    {
                        return cell.StringCellValue;
                    }
                case NPOI.SS.UserModel.CellType.Blank:
                    return "";
                case NPOI.SS.UserModel.CellType.Boolean:
                    return cell.BooleanCellValue ? "1" : "0";
                case NPOI.SS.UserModel.CellType.Error:
                    return "";
            }
            return "";
        }
    }
}
