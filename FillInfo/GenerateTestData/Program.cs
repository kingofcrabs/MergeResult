using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace GenerateTestData
{
    class Program
    {
        static void Main(string[] args)
        {
            List<string> lines = new List<string>();
            lines.Add("条");
            for(int i = 0; i < 16; i++)
            {
                string orgBarcode = string.Format("15P14859{0:D2}", i + 1);
                string s = string.Format("行{0}", i + 1);
                s += "," + orgBarcode + "-A";
                s += "," + orgBarcode + "-B";
                s += "," + orgBarcode + "-1";
                s += "," + orgBarcode + "-2";
                s += "," + orgBarcode + "-3";
                s += "," + orgBarcode + "-4";
                s += "," + orgBarcode + "-5";
                lines.Add(s);
            }
            File.WriteAllLines("f:\\test.txt", lines);
        }
    }
}
