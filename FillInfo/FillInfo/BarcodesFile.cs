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
            List<List<string>> eachLabwareBarcodes = new List<List<string>>();
            bool bFirstLineContainsValue = true;
            foreach(string line in lines)
            {
                string[] strs = line.Split(',');
                strs = strs.Skip(1).ToArray();
                var suffixA = strs.Where(x => x.Contains("A"));
                int cols = suffixA.Count();
                strs = strs.Where(x => GetDigitalSuffix(x)).ToArray();
                if(cols == 0)
                {
                    continue;
                }
                slices = strs.Count() / cols;
                if (bFirstLineContainsValue)
                {
                    for (int i = 0; i < cols; i++)
                        eachLabwareBarcodes.Add(new List<string>());
                    bFirstLineContainsValue = false;
                }
                CheckBarcodes(strs, cols, slices);
                for (int i = 0; i < cols; i++ )
                {
                    var tmpStrs = strs.Take(slices).ToList();
                    eachLabwareBarcodes[i].AddRange(tmpStrs);
                    strs = strs.Skip(slices).ToArray();

                }
                
            }
            foreach(List<string> barcodesSameLabware in eachLabwareBarcodes)
            {
                allBarcodes.AddRange(barcodesSameLabware);
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
                        throw new Exception (string.Format("barcode:{0} is incorrect, expected：{1} at column:{2} slice:{3}",curBarcode,expectBarcode,col+1,slice+1));
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
