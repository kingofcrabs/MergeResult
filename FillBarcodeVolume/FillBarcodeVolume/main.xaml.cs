using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using WinForms = System.Windows.Forms;

namespace FillBarcodeVolume
{
    /// <summary>
    /// Interaction logic for SelectFolder.xaml
    /// </summary>
    public partial class FillBarcodeVolume : Window
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
      
        public FillBarcodeVolume()
        {
            InitializeComponent();
        }

        internal void GetFilesInfo(List<string> barcodeFiles, List<string> volumeFiles, ref string excelFile)
        {
            throw new NotImplementedException();
        }

        private void btnMerge_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                MergeFiles();
            }
            catch(Exception ex)
            {
                SetInfo(ex.Message);
                log.Info(ex.Message + ex.StackTrace);
            }
        }

        private void MergeFiles()
        {
            log.Info("Merge Files");
            string errMsg = "";
            bool readyForMerge = CheckReady(ref errMsg);
            if (!readyForMerge)
            {
                SetInfo(errMsg);
                return;
            }

            int nBarcodeFileCnt = BarcodeFiles.Count;
            List<string> allBarcodes = new List<string>();
            List<string> allVolumes = new List<string>();
            BarcodesFileReader barcodesFileReader = new BarcodesFileReader();
            VolumeFileReader volumeFileReader = new VolumeFileReader();
            int slices = 0;
            for (int i = 0; i < nBarcodeFileCnt; i++)
            {
                string barcodeFilePath = BarcodeFiles[i];
                string volumeFilePath = VolumeFiles[i];
                var barcodes = barcodesFileReader.Read(barcodeFilePath, ref slices);
                var volumes = volumeFileReader.Read(volumeFilePath, slices);
                if(barcodes.Count() != volumes.Count())
                {
                    string tmpError = string.Format("barcode count:{2} != volume count:{3} at file:{0} & {1}", barcodeFilePath, volumeFilePath, barcodes.Count(), volumes.Count());
                    SetInfo(tmpError);
                    log.InfoFormat(tmpError);
                    return;
                }
                allBarcodes.AddRange(barcodes);
                allVolumes.AddRange(volumes);
            }
            List<SampleInfo> sampleInfos = GetSampleInfos(allBarcodes, allVolumes);
            ExcelHelper.WriteResult(sampleInfos, DstFile);
        }

        private List<SampleInfo> GetSampleInfos(List<string> allBarcodes, List<string> allVolumes)
        {
            List<SampleInfo> samplesInfo = new List<SampleInfo>();
            int cnt = allBarcodes.Count;
            for(int i = 0; i< cnt; i++)
            {
                string barcode = allBarcodes[i];
                string volume = allVolumes[i];
                string orgBarcode = GetOrgBarcode(barcode);
                samplesInfo.Add(new SampleInfo(barcode, orgBarcode, volume));
            }
            return samplesInfo;
        }

        private string GetOrgBarcode(string barcode)
        {
            int index = barcode.IndexOf('-');
            return barcode.Substring(0, index);
        }

        public List<string> BarcodeFiles { get; set; }
        public List<string> VolumeFiles { get; set; }
        public string DstFile { get; set; }

        private void btnBrowse_Click(object sender, RoutedEventArgs e)
        {
            btnMerge.IsEnabled = false;
            var dialog = new WinForms.FolderBrowserDialog();
            var result = dialog.ShowDialog();
            if (result != WinForms.DialogResult.OK)
                return;

            string folder = dialog.SelectedPath;
            DirectoryInfo dirInfo = new DirectoryInfo(folder);
            var files = dirInfo.EnumerateFiles("*.csv").Select(x=>x.FullName);
            SeparateFilesByType(files);
            DstFile = dirInfo.EnumerateFiles(".xlsx").First().FullName;
            BarcodeFiles.Sort();
            VolumeFiles.Sort();
            lstBarcodes.ItemsSource = BarcodeFiles;
            lstVolumes.ItemsSource = VolumeFiles;
            
            //检查
            string errMsg = "";
            bool readyForMerge = CheckReady(ref errMsg);
            if(!readyForMerge)
            {
                SetInfo(errMsg);
            }
            btnMerge.IsEnabled = readyForMerge;
            
        }

        private bool CheckReady(ref string errMsg)
        {
            if (BarcodeFiles.Count == 0)
            {
                errMsg = "未找到条码文件！";
                return false;
            }
            if (BarcodeFiles.Count != VolumeFiles.Count)
            {
                errMsg = "条码文件数与体积文件数不相等！";
                return false;
            }
            if (DstFile == "")
            {
                errMsg = "未找到目标文件！";
                return false;
            }
            return true;
        }

        private void SetInfo(string txt, bool bRed = true)
        {
            txtInfo.Text = txt;
            txtInfo.Foreground = new SolidColorBrush(bRed ? Colors.Red : Colors.Black);
        }

        private void SeparateFilesByType(IEnumerable<string> files)
        {
            BarcodeFiles.Clear();
            VolumeFiles.Clear();
            foreach(string file in files)
            {
                bool isBarcode = File.ReadLines(file).First().Contains("条");
                if(isBarcode)
                {
                    BarcodeFiles.Add(file);
                }
                else
                {
                    VolumeFiles.Add(file);
                }
            }
        }
    }
}
