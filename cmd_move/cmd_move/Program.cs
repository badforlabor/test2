using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace cmd_move
{
    class Program
    {
        static void Main(string[] args)
        {
            // 从一个目录中检索某个文件，然后按照目录结构，移动到另一个文件
            const string Src = @"D:\workspace\war\shared\02-Editor\Assets\";
            const string Target = @"D:\workspace\war\temp\";
            string[] fileFilterArray = { "*.png", "*.fbx" };

            foreach (var fileFilter in fileFilterArray)
            {
                string[] files = Directory.GetFiles(Src, fileFilter, SearchOption.AllDirectories);
                foreach (var f in files)
                {
                    string targetFile = Target + f.Replace(Src, "");
                    Directory.CreateDirectory(Path.GetDirectoryName(targetFile));

                    File.Move(f, targetFile);
                    Console.WriteLine("move:" + f + ", to" + targetFile);
                }
            }
        }
    }
}
