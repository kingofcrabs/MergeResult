using Microsoft.Office.Interop.Excel;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Reflection;
using System.Text;

namespace FillInfo
{
    class ExcelHelper
    {
        public static void WriteResult( List<SampleInfo> allVolumeInfos, string sFile)
        {
            Application excelApp = new Application();
            excelApp.DisplayAlerts = false;
            Workbook workBook = excelApp.Workbooks.Open(sFile);
            try
            {
                Worksheet xlsWs = null;
                Range ExcelCellText;
                xlsWs = (Worksheet)workBook.Worksheets.get_Item(1);
                string startCell = ConfigurationManager.AppSettings["startCell"];
                ExcelCellText = xlsWs.get_Range(startCell, Missing.Value);
                ExcelCellText = ExcelCellText.get_Resize(allVolumeInfos.Count, 3);
                string[,] myArray = new string[allVolumeInfos.Count, 3];
                for (int r = 0; r < allVolumeInfos.Count; r++)
                {
                    myArray[r, 0] = allVolumeInfos[r].orgBarcode;
                    myArray[r, 1] = allVolumeInfos[r].curBarcode;
                    myArray[r, 2] = allVolumeInfos[r].volume;
                }

                System.Threading.Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("en-US");
                ExcelCellText.set_Value(Missing.Value, myArray);
                workBook.Save();
            }
            finally
            {
                workBook.Close();
                excelApp.Quit();
            }
            
        }
    }

    class SampleInfo
    {
        public string orgBarcode;
        public string curBarcode;
        public string volume;


        public SampleInfo(string barcode, string orgBarcode, string volume)
        {
            // TODO: Complete member initialization
            this.orgBarcode = orgBarcode;
            this.curBarcode = barcode;
            this.volume = volume;
        }
    }
}
