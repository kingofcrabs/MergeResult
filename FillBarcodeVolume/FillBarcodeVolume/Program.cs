using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

namespace FillBarcodeVolume
{
    class Program
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        [STAThread]
        static void Main(string[] args)
        {
            try
            {
                var form = new  FillBarcodeVolume();
                form.ShowDialog();
            }
            catch(Exception ex)
            {
                log.Error(ex.Message + ex.StackTrace);
            }
        }

    }
}
