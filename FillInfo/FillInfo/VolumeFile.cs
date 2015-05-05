using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace FillInfo
{
    class VolumeFileReader
    {
        int m_Slices = 0;
        private List<string> Parse(IEnumerable<string> lines)
        {
            if (lines.Count() % m_Slices != 0)
            {
                throw new Exception("incorrect line count, should be: " + m_Slices.ToString());
            }

            List<string> allVolumes = new List<string>();
            while(lines.Count() > 0)
            {
                var thisBatchLines = lines.Take(m_Slices);
                lines = lines.Skip(m_Slices);
                bool bLastBatch = lines.Count() == 0;
                allVolumes.AddRange(GetVolThisRegion(thisBatchLines,bLastBatch));
            }
            return allVolumes;
        }

        private IEnumerable<string> GetVolThisRegion(IEnumerable<string> thisBatchLines, bool isLastBatch)
        {
            List<List<string>> volumes = new List<List<string>>();
            foreach(string line in thisBatchLines)
            {
                volumes.Add(line.Split(',').Select(x => ConvertFormat(x,isLastBatch)).ToList());
            }
            int nSlices = volumes.Count;
            int batchTips = volumes[0].Count;
            List<string> result = new List<string>();
            for( int tipIndex = 0; tipIndex < batchTips; tipIndex++)
            {
                for(int i = 0; i< nSlices; i++)
                {
                    result.Add(volumes[i][tipIndex]);
                }
            }
            return result;
        }

        private string ConvertFormat(string x,bool isLastBatch)
        {
            if (x == "")
                return x;
            int dotIndex = x.IndexOf('.');
            string formatedStr = x.Substring(0, dotIndex);
            double val = double.Parse(formatedStr);
            if(val <0 )
            {
                formatedStr = isLastBatch ? "" : "0";
            }
            return formatedStr;
        }

        public IEnumerable<string> Read(string file, int slices)
        {
            m_Slices = slices;
            var lines = File.ReadAllLines(file);
            lines = lines.Skip(1).ToArray();
            return Parse(lines);
        }
    }
}
