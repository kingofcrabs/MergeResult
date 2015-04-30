using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FillBarcodeVolume
{
    class Program
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        static void Main(string[] args)
        {
            try
            {
                FillInfo();
            }
            catch(Exception ex)
            {
                log.Error(ex.Message + ex.StackTrace);
            }
            Console.WriteLine("Press any key to continue...");
            Console.ReadKey();
        }

        private static void FillInfo()
        {
            BarcodesFile barcodesFile = new BarcodesFile();
            int slices = 0;
            string barcodeFile = @"F:\Projects\MergeResult.git\trunk\data\防错1.csv";
            log.InfoFormat("Read file: {0}", barcodeFile);
            var barcodes = barcodesFile.Read(barcodeFile, ref slices);
            VolumeFile volumeFile = new VolumeFile();
            string volumeFilePath = @"F:\Projects\MergeResult.git\trunk\data\体积数据1.csv";
            var lines = volumeFile.Read(volumeFilePath, slices);
            volumeFile.Parse(lines);
        }
    }
}
