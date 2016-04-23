using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

namespace AutoGenProtocol
{
    class Program
    {
        static void Main(string[] args)
        {
            XmlDocument xml = new XmlDocument();
            xml.Load("D:\\liubo\\github2\\test2\\AutoGenProtocol\\test.xml");

            XmlNode env = xml.SelectSingleNode("env");
            if (env != null)
            {
                Console.WriteLine("env-log = " + env.Attributes["log"].Value);
            }

            Console.WriteLine("root-name = " + xml.DocumentElement.Name);
            
        }
    }
}
