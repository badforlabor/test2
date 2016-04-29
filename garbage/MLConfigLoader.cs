using UnityEngine;
using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Collections;
using System.Collections.Generic;

// !< Discuss:
// !< 1.为何不直接导出C#代码(类似lua那样),省去读取csv的时间?
// !<   A:因为那样更新csv表格在理论上不可行了
// !< 2.当前有几种读取表格方式?
// !<   A:一种方式:使用MLConfigLoader进行加载,自动生成的数据结构只是减少了载入数据结构的重复性劳动.
// !< 3.TODO
// !< ->如何更好的省时省力???
namespace MLGame
{
    public class ConfigLine
    {
        // !< parent
        protected ConfigFile mFile;
        // !< 数据行ID
        protected string mKey;
        // !< 数据行
        protected ArrayList mDatas;

        public string Key
        {
            get
            {
                return mKey;
            }
        }

        public ConfigLine(ConfigFile file, string key, ArrayList data)
        {
            mFile = file;
            mKey = key;
            mDatas = data;
        }

        public T GetColumn<T>(string column)
        {
            int index = mFile.GetTitleIdx(column);
            if (index < 0 || index >= mDatas.Count)
            {
                // !< WARN:不能使用LogError()否则程序直接终止
                throw new Exception("[load config:" + mFile.FileName + "] error, column \'" + column + "\' doesn't exist! idx=" + index);
            }
            return MLPrefabUtil.ConvertValue<T>(mDatas[index]);
        }

        public T GetColumn<T>(int index)
        {
            if (index < 0 || index >= mDatas.Count)
            {
                // !< WARN:不能使用LogError()否则程序直接终止
                throw new Exception("[load config:" + mFile.FileName + "] error, index \'" + index + "\' out of range! idx=" + index);
            }
            return MLPrefabUtil.ConvertValue<T>(mDatas[index]);
        }

        public int ColumnCount()
        {
            return mDatas.Count;
        }
    }

    // 文件载入内存初始化对象后,是不是就可以删除了?
    public class ConfigFile
    {
        // csv文件名
        protected string mFileName;
        // csv标题,用于查找时快速找到索引
        protected Dictionary<string, int> mTitleIndexs;
        // csv数据部分,用于查找内容
        protected Dictionary<string, ConfigLine> mDatas;
        protected Dictionary<string, ArrayList> mDataLists;

        public string FileName
        {
            get
            {
                return mFileName;
            }
        }
        public ConfigFile(string file)
        {
            mFileName = file;
            mTitleIndexs = new Dictionary<string, int>();
            mDataLists = new Dictionary<string, ArrayList>();
            mDatas = new Dictionary<string, ConfigLine>();
        }

        public void Reset()
        {
            mTitleIndexs.Clear();
            mDataLists.Clear();
            mDatas.Clear();
        }

        public void SetTitles(string[] titles)
        {
            for (int i = 0; i < titles.Length; ++i )
            {
                mTitleIndexs.Add(titles[i], i);
            }
        }

        public int GetTitleIdx(string title)
        {
            int idx = -1;
            if (mTitleIndexs.TryGetValue(title,out idx))
            {
                return idx;
            }
            return -1;
        }

        public void AddData(string key, ArrayList data)
        {
            if (mDatas.ContainsKey(key))
            {
                throw new Exception(String.Format("表{0}加载失败, ID重了 = {1}", mFileName, key));
            }
            else
            {
                mDatas.Add(key, new ConfigLine(this, key, data));
                mDataLists.Add(key, data);
            }
        }

        public T FindData<T>(string key, string colum)
        {
            ConfigLine line;
            if (!mDatas.TryGetValue(key, out line))
            {
                throw new Exception("[load config:" + FileName + "] error, data \'" + key + "\' doesn't exit at all!");
            }

            return line.GetColumn<T>(colum);
        }

        public ConfigLine FindData(string key)
        {
            ConfigLine line;
            if (!mDatas.TryGetValue(key, out line))
            {
                return null;
            }
            return line;
        }

        // !< 返回的是ConifigLine的列表
        public Dictionary<string, ConfigLine> GetLines()
        {
            return mDatas;
        }

        [Obsolete("this function was deprecated, use GetLines() instead!")]
        public Dictionary<string, ArrayList> GetData()
        {
            return mDataLists;
        }
    }

    // !< WARN:当前读取CSV表走的是字符串处理,效率不咋高,同时,行内的内容拆分走的是正则表达式.
    // !<   未来优化时,可以考虑将所有表格序列化为txt再读取!
    // !< WARN:当前解析CSV一行时,是拆分为一个一个字段(也可以走序列化)后,再进行类型转化并传递给对应的成员变量赋值!!
    // !<   未来优化时,可以改为运行时再进行类型转换!!!(或者也可以考虑使用序列化解决)
    public class MLConfigLoader : MLINotObfuscator
    {
        private static Dictionary<string, ConfigFile> mConfigFiles = new Dictionary<string, ConfigFile>();
        private static Regex mCSVRegex = new Regex(@"(((?<x>(?=[,\r\n]+))|""(?<x>([^""]|"""")+)""|(?<x>[^,\r\n]+)),?)", RegexOptions.ExplicitCapture);
        public static float parseTime = 0f;
        public static float readTime = 0f;
        public static string[] SplitSingleLine(string line)
        {
            return (from Match m in mCSVRegex.Matches(line) select m.Groups[1].Value).ToArray();
        }

        public static ConfigFile LoadSingleConfigure(string filename)
        {
            ConfigFile file = null;

            float delta = Time.realtimeSinceStartup;
            TextAsset config = ResourceLoad.LoadAsset<TextAsset>("Config/" + filename);
            delta = Time.realtimeSinceStartup - delta;
            readTime += delta;
            if (config != null)
            {
                file = LoadSingleConfigure(filename, config.text);
            }

            return file;
        }

        public static ConfigFile LoadSingleConfigure(string filename, string data)
        {
            // !< WARN : 由于是字符处理方式,需要保证输入的data为UTF-8格式
            // 拆分为行
            string[] lineArray = data.Split("\n"[0]);

            if (lineArray.Length < 1)
            {
                // error
                return null;
            }
            ConfigFile fileData = new ConfigFile(filename);

            // 第一行为csv头
            float delta = Time.realtimeSinceStartup;
            string[] header = SplitSingleLine(lineArray[0]);
            fileData.SetTitles(header);

            for (int i = 1; i < lineArray.Length;++i )
            {
                if (lineArray[i] != "")
                {
                    string[] line = SplitSingleLine(lineArray[i]);
                    ArrayList dataLine = ArrayList.Adapter(line);
                    fileData.AddData(line[0], dataLine);
                }
            }

            delta = Time.realtimeSinceStartup - delta;
            parseTime += delta;

            return fileData;
        }

        //不建议对外使用，未来方便统一清理掉config的引用
        private static ConfigFile GetConfigure(string filename)
        {
            ConfigFile result = null;
            if (!mConfigFiles.TryGetValue(filename, out result))
            {
                result = LoadSingleConfigure(filename);
                mConfigFiles.Add(filename, result);
            }

            return result;
        }

        public static string FindConfigData(string filename, int key, string column)
        {
            return FindConfigData<string>(filename, key.ToString(), column);
        }

        public static T FindConfigData<T>(string filename, string key, string column)
        {
            ConfigFile result = GetConfigure(filename);
            if (result == null)
                return default(T);

            return result.FindData<T>(key, column);
        }

        public static T FindConfigData<T>(string filename, int key, string column)
        {
            ConfigFile result = GetConfigure(filename);
            if (result == null)
                return default(T);

            return result.FindData<T>(key.ToString(), column);
        }
    }
}
