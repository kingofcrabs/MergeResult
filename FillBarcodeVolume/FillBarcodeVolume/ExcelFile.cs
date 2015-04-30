using Microsoft.Office.Interop.Excel;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Reflection;
using System.Text;

namespace FillBarcodeVolume
{
    class ExcelFile
    {
        public void WriteResult( List<VolumeInfo> allVolumeInfos, string sFile)
        {

            Application excelApp = new Application();
            Workbook workBook = excelApp.Workbooks.Add();

            Worksheet xlsWs = null;
            Range ExcelCellText;
            workBook.Worksheets.Add();
            xlsWs = (Worksheet)workBook.Worksheets.get_Item(1);
            string startCell = ConfigurationManager.AppSettings["startCell"];
            ExcelCellText = xlsWs.get_Range("startCell", Missing.Value);
            ExcelCellText = ExcelCellText.get_Resize(allVolumeInfos.Count, 3);
            string[,] myArray = new string[allVolumeInfos.Count, 3];
            for (int r = 0; r < allVolumeInfos.Count; r++)
            {
                myArray[r, 0] = allVolumeInfos[r].orgBarcode;
                myArray[r, 1] = allVolumeInfos[r].resBarcode;
                myArray[r, 2] = allVolumeInfos[r].volume;
            }

            System.Threading.Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("en-US");
            ExcelCellText.set_Value(Missing.Value, myArray);
            workBook.SaveAs(sFile);
            workBook.Close();
            excelApp.Quit();
        }
    }

    class VolumeInfo
    {
        public string orgBarcode;
        public string resBarcode;
        public string volume;
    }
}
