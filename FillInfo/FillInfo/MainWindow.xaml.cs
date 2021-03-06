﻿using FillInfo.Properties;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using WinForms = System.Windows.Forms;

namespace FillInfo
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window,INotifyPropertyChanged
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        public event PropertyChangedEventHandler PropertyChanged;
        private string dstFile;
        public MainWindow()
        {
            InitializeComponent();
            txtExcelPath.DataContext = this;
            lblVersion.Content = strings.version;
        }

        private void btnMerge_Click(object sender, RoutedEventArgs e)
        {
            btnBrowse.IsEnabled = false;
            btnMerge.IsEnabled = false;
            SetInfo("开始merge！", false);
            
            ThreadStart thstart = new ThreadStart(MergeFiles);
            Thread th = new Thread(thstart);
            th.Start();
           
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
                if (barcodes.Count() != volumes.Count())
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
            this.Dispatcher.Invoke(new Action(()=>{
                SetInfo("已经保存到excel！", false);
                btnBrowse.IsEnabled = true;
                btnMerge.IsEnabled = true;
            }));
          
        }

        private List<SampleInfo> GetSampleInfos(List<string> allBarcodes, List<string> allVolumes)
        {
            List<SampleInfo> samplesInfo = new List<SampleInfo>();
            int cnt = allBarcodes.Count;
            for (int i = 0; i < cnt; i++)
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
        public string DstFile
        {
            get
            {
                return dstFile;
            }
            set
            {
                dstFile = value;
                OnPropertyChanged("DstFile");
            }
        }
        private void OnPropertyChanged(string prop)
        {
           if( PropertyChanged != null )
           {
              PropertyChanged(this, new PropertyChangedEventArgs(prop));
           }
        }
 

        private void btnBrowse_Click(object sender, RoutedEventArgs e)
        {
            btnMerge.IsEnabled = false;
            SetInfo("", false);
            var dialog = new WinForms.FolderBrowserDialog();
            if (Properties.Settings.Default.lastFolder != "")
                dialog.SelectedPath = Properties.Settings.Default.lastFolder;
            var result = dialog.ShowDialog();
            if (result != WinForms.DialogResult.OK)
                return;

            string folder = dialog.SelectedPath;
            Properties.Settings.Default.lastFolder = folder;
            Properties.Settings.Default.Save();
            DirectoryInfo dirInfo = new DirectoryInfo(folder);
            var files = dirInfo.EnumerateFiles("*.csv").Select(x => x.FullName);
            SeparateFilesByType(files);
            var excels = dirInfo.EnumerateFiles("*.xlsx");
            if (excels != null && excels.Count() > 0)
                DstFile = excels.First().FullName;
            else
            {
                SetInfo("未能找到excel文件！");
                return;
            }
              
            BarcodeFiles.Sort();
            VolumeFiles.Sort();
            lstBarcodes.ItemsSource = BarcodeFiles;
            lstVolumes.ItemsSource = VolumeFiles;

            //检查
            string errMsg = "";
            bool readyForMerge = CheckReady(ref errMsg);
            if (!readyForMerge)
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
            BarcodeFiles = new List<string>();
            VolumeFiles = new List<string>();
            foreach (string file in files)
            {
                string firstLine = File.ReadLines(file).First();
                bool isBarcode = firstLine.Contains("条") || firstLine.Contains("?");
                if (isBarcode)
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
