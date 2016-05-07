/****************************************************************************
Copyright (c) 2013-2025,大连-游你酷伴.
 This is not a free-ware .DO NOT use it without any authorization.
 * 坚持做有意思的游戏
 * 
 * date : 5/7/2016 7:11:37 AM
 * author : Labor
 * purpose : 
****************************************************************************/
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace SHGame
{
    public static class split_string
    {        // !< 拆分字符串,并返回为指定类型
        public static List<T> SplitString<T>(String total)
        {
            return SplitString<T>(total, ";");
        }
        public static List<T> SplitString<T>(String total, String seperator)
        {
            List<T> ret = new List<T>();
            if (total != null && total != "")
            {
                String[] values = Regex.Split(total, seperator);
                for (int i = 0; i < values.Length; i++)
                {
                    T tTemp = (T)Convert.ChangeType(values[i], typeof(T));
                    ret.Add(tTemp);
                }
            }

            return ret;
        }
    }
}
