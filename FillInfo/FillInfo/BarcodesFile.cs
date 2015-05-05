using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace FillInfo
{
    class BarcodesFileReader
    {
        public List<string> Read(string sFile,ref int slices)
        {
            List<string> results = new List<string>();
            var lines = File.ReadAllLines(sFile);
            lines = lines.Skip(1).ToArray();
            lines = lines.Where(x => x.Contains("A")).ToArray();
            List<string> allBarcodes = new List<string>();
            foreach(string line in lines)
            {
                string[] strs = line.Split(',');
                strs = strs.Skip(1).ToArray();
                var suffixA = strs.Where(x => x.Contains("A"));
                int cols = suffixA.Count();
                strs = strs.Where(x => GetDigitalSuffix(x)).ToArray();
                slices = strs.Count() / cols;
                CheckBarcodes(strs, cols, slices);
                allBarcodes.AddRange(strs);
            }
            
            return allBarcodes;
        }

        private void CheckBarcodes(string[] strs, int cols, int slices)
        {
            if (strs.Count() != cols * slices)
                throw new Exception("barcodeCount != slice * column");
            for(int col = 0; col < cols; col++)
            {
                string template = strs[col * slices];
                template = template.Substring(0, template.IndexOf('-'));
                for( int slice = 0; slice < slices; slice++)
                {
                    string curBarcode = strs[col * slices + slice];
                    string expectBarcode = string.Format("{0}-{1}",template,slice+1);
                    if(curBarcode != expectBarcode)
                    {
                        throw new Exception (string.Format("barcode:{0} is incorrect, expected：{1}",curBarcode,expectBarcode));
                    }
                }
            }
        }

        private bool GetDigitalSuffix(string s)
        {
            if (!s.Contains('-'))
                return false;
            string[] strs = s.Split('-');
            return char.IsDigit(strs[1].First());
        }
    }
}
