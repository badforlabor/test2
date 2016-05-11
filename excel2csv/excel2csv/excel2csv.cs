/****************************************************************************
Copyright (c) 2013-2025,大连-游你酷伴.
 This is not a free-ware .DO NOT use it without any authorization.
 * 坚持做有意思的游戏
 * 
 * date : 5/9/2016 11:25:44 PM
 * author : Labor
 * purpose : 
****************************************************************************/
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.IO;
using NPOI.HSSF.UserModel;

namespace SHGame
{
    public class Excel2Csv
    {
        public void Test()
        {
            var ret = GetExcelData("../../res/excel/client.xls");
        }
        System.Data.DataTable GetExcelData(string excelFilePath)
        {
            string shortName = Path.GetFileNameWithoutExtension(excelFilePath);
            //path = Application.dataPath + "/../Builder/builder_config.xls";

            // !< 说明:客户端的表读取,就不走服务器那样的复杂处理啦..
            FileStream fs = File.Open(excelFilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            HSSFWorkbook wk = new HSSFWorkbook(fs);
            for (int i = 0; i < wk.NumberOfSheets; i++)
            {
                HSSFSheet sheet = wk.GetSheetAt(i) as HSSFSheet;
                for (int j = sheet.FirstRowNum; j <= sheet.LastRowNum; j++)
                {
                    HSSFRow row = sheet.GetRow(j) as HSSFRow;
                    for (int k = row.FirstCellNum; k < row.LastCellNum; k++)
                    {
                        HSSFCell cell = row.GetCell(k) as HSSFCell;
                        cell.ToString();
                    }
                }
            }

            return null;
        }
    }
}
