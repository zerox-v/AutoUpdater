using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using FSLib.App.AutoUpdater;
using System.IO;

namespace FSLib.App.AutoUpdater.UI
{
    /// <summary>
    /// Interaction logic for UpdateInfo.xaml
    /// </summary>
    public partial class UpdateInfoWin : WindowBase
    {
        public UpdateInfoWin()
        {
            InitializeComponent();
            this.Loaded += new RoutedEventHandler(UpdateInfoWin_Loaded);
        }

        void UpdateInfoWin_Loaded(object sender, RoutedEventArgs e)
        {
            this.Title = "软件更新提醒";
            //MessageBox.Show(Updater.Instance.CurrentVersion.ToString()),
            //if (MessageBox.Show("发现新版本,更新程序将自动关闭当前运用程序,如果必要,请保存当前文档!是否现在更新?", "软件更新提醒", MessageBoxButton.YesNo) != MessageBoxResult.Yes)
            //{
            //    return;
            //}
            //return;
            //txtDes.Text = Info.Desc;

        }

        public UpdateInfo Info
        {
            get;
            set;
        }

        public string CallExeName
        {
            get;
            set;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            //更新程序复制到缓存文件夹
            string startUpDir1 = System.IO.Path.Combine(System.Reflection.Assembly.GetEntryAssembly().Location.Substring(0, System.Reflection.Assembly.GetEntryAssembly().Location.LastIndexOf(System.IO.Path.DirectorySeparatorChar)));
            string startUpDir2 = System.IO.Path.Combine(startUpDir1.Substring(0, startUpDir1.LastIndexOf(System.IO.Path.DirectorySeparatorChar)));
            string updateFileDir = System.IO.Path.Combine(startUpDir2, "Update");
            if (!Directory.Exists(updateFileDir))
            {
                Directory.CreateDirectory(updateFileDir);
            }
            string exeDir = Guid.NewGuid().ToString();
            string updateExeDir = System.IO.Path.Combine(updateFileDir, exeDir);
            if (!Directory.Exists(updateExeDir))
            {
                Directory.CreateDirectory(updateExeDir);
            }

            string exePath = System.IO.Path.Combine(updateExeDir, "AutoUpdater.exe");
            File.Copy(System.IO.Path.Combine(startUpDir1, "AutoUpdater.exe"), exePath, true);

            var info = new System.Diagnostics.ProcessStartInfo(exePath);
            info.UseShellExecute = true;
            info.WorkingDirectory = exePath.Substring(0, exePath.LastIndexOf(System.IO.Path.DirectorySeparatorChar));
            info.Arguments = "update " + CallExeName + " " + updateFileDir + " " + startUpDir1 + " " + exePath;
            System.Diagnostics.Process.Start(info);
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
