/****************************************************************************
Copyright (c) 2013-2025,大连-游你酷伴.
 This is not a free-ware .DO NOT use it without any authorization.
 * 坚持做有意思的游戏
 * 
 * date : 5/9/2016 10:00:19 PM
 * author : Labor
 * purpose : xml文件对应的数据结构
****************************************************************************/
using System;
using System.Collections;
using System.Collections.Generic;

namespace SHGame
{
    public class XMLStruct
    {
        public class Column
        {
            public string ValueType { get; set; }
            public string ValueName { get; set; }
            public int Denominator { get; set; }
            public int NotNull { get; set; }
            public string Ignore { get; set; }
            public string Comment { get; set; }
        }
        public class Sheet
        {
            public string Template;
        }
        public class Excel
        {
            public string FileName { get; set; }
            public Sheet[] sheets { get; set; }
        }
    }


    public class xml_struct
    {
    }

}
