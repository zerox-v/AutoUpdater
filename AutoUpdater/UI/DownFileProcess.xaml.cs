using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;

namespace AutoUpdater.UI
{
    public partial class DownFileProcess : Window
    {
        private string updateFileDir;//更新文件存放的文件夹
        private string callExeName;
        private string appDir;
        private string appName;
        private string appVersion;
        private string desc;
        public DownFileProcess()
        {
            InitializeComponent();
        }

        public DownFileProcess(string callExeName, string updateFileDir, string appDir, string appName, string appVersion, string desc)
        {
            InitializeComponent();
            Screen[] screens = Screen.AllScreens;
            var subScreen = (from o in screens where o.Primary == false select o).ToList<Screen>();
            if (subScreen.Count > 0)
            {
                var subscreen = subScreen[0];
               var  mswa = subscreen.WorkingArea;
             
               
                this.Left = mswa.Left;
                this.Top = mswa.Top;

                this.Loaded += (sender, e) =>
                {
                    this.WindowStartupLocation = WindowStartupLocation.CenterScreen;
                };
              

            }
            this.callExeName = System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(callExeName));
            this.updateFileDir = System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(updateFileDir));
            this.appDir = System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(appDir));
            this.appName = System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(appName));
            this.appVersion = System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(appVersion));

            string sDesc = System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(desc));
            if (sDesc.ToLower().Equals("null"))
            {
                this.desc = "";
            }
            else
            {
                this.desc = sDesc;
            }
            this.txtDes.Text = this.desc;
            this.tb_ver.Text = this.appVersion;
            this.txtDes.Text = sDesc;
        }

        public void DownloadUpdateFile()
        {
            string url = Constants.RemoteUrl + callExeName + "/update.zip";
            var client = new System.Net.WebClient();
            client.DownloadProgressChanged += (sender, e) =>
            {
                UpdateProcess(e.BytesReceived, e.TotalBytesToReceive);
            };
            client.DownloadDataCompleted += (sender, e) =>
            {
                string zipFilePath = System.IO.Path.Combine(updateFileDir, "update.zip");
                byte[] data = e.Result;
                BinaryWriter writer = new BinaryWriter(new FileStream(zipFilePath, FileMode.OpenOrCreate));
                writer.Write(data);
                writer.Flush();
                writer.Close();

                System.Threading.ThreadPool.QueueUserWorkItem((s) =>
                {
                    Action f = () =>
                    {
                        txtProcess.Text = "开始更新程序...";

                    };
                    this.Dispatcher.Invoke(f);

                    string tempDir = System.IO.Path.Combine(updateFileDir, "temp");
                    if (!Directory.Exists(tempDir))
                    {
                        Directory.CreateDirectory(tempDir);
                    }
                    UnZipFile(zipFilePath, tempDir);

                    //移动文件
                    //App
                    if (Directory.Exists(System.IO.Path.Combine(tempDir, "App")))
                    {
                        CopyDirectory(System.IO.Path.Combine(tempDir, "App"), appDir);
                    }

                    f = () =>
                    {
                        txtProcess.Text = "更新完成!";

                        try
                        {
                            //清空缓存文件夹
                            string rootUpdateDir = updateFileDir.Substring(0, updateFileDir.LastIndexOf(System.IO.Path.DirectorySeparatorChar));
                            foreach (string p in System.IO.Directory.EnumerateDirectories(rootUpdateDir))
                            {
                                if (!p.ToLower().Equals(updateFileDir.ToLower()))
                                {
                                    System.IO.Directory.Delete(p, true);
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            //MessageBox.Show(ex.Message);
                        }

                    };
                    this.Dispatcher.Invoke(f);

                    try
                    {
                        f = () =>
                        {
                            AlertWin alert = new AlertWin("更新完成,是否现在启动软件?") { WindowStartupLocation = WindowStartupLocation.CenterOwner, Owner = this };
                            alert.Title = "更新完成";
                            alert.YesBtnEventCallBack += () =>
                            {
                                //启动软件
                                string exePath = System.IO.Path.Combine(appDir, callExeName + ".exe");
                                var info = new System.Diagnostics.ProcessStartInfo(exePath);
                                info.UseShellExecute = true;
                                info.WorkingDirectory = appDir;// exePath.Substring(0, exePath.LastIndexOf(System.IO.Path.DirectorySeparatorChar));
                                System.Diagnostics.Process.Start(info);
                            };
                            alert.Width = 400;
                            alert.Height = 300;
                            alert.ShowDialog();

                            this.Close();
                        };
                        this.Dispatcher.Invoke(f);
                    }
                    catch (Exception ex)
                    {
                        //MessageBox.Show(ex.Message);
                    }
                });

            };
            client.DownloadDataAsync(new Uri(url));
        }

        private static void UnZipFile(string zipFilePath, string targetDir)
        {
            ICCEmbedded.SharpZipLib.Zip.FastZipEvents evt = new ICCEmbedded.SharpZipLib.Zip.FastZipEvents();
            ICCEmbedded.SharpZipLib.Zip.FastZip fz = new ICCEmbedded.SharpZipLib.Zip.FastZip(evt);
            fz.ExtractZip(zipFilePath, targetDir, "");
        }

        public void UpdateProcess(long current, long total)
        {
            string status = (int)((float)current * 100 / (float)total) + "%";
            this.txtProcess.Text = status;
            rectProcess.Width = ((float)current / (float)total) * bProcess.ActualWidth;
        }

        public void CopyDirectory(string sourceDirName, string destDirName)
        {
            try
            {
                if (!Directory.Exists(destDirName))
                {
                    Directory.CreateDirectory(destDirName);
                    File.SetAttributes(destDirName, File.GetAttributes(sourceDirName));
                }
                if (destDirName[destDirName.Length - 1] != Path.DirectorySeparatorChar)
                    destDirName = destDirName + Path.DirectorySeparatorChar;
                string[] files = Directory.GetFiles(sourceDirName);
                foreach (string file in files)
                {
                    File.Copy(file, destDirName + Path.GetFileName(file), true);
                    File.SetAttributes(destDirName + Path.GetFileName(file), FileAttributes.Normal);
                }
                string[] dirs = Directory.GetDirectories(sourceDirName);
                foreach (string dir in dirs)
                {
                    CopyDirectory(dir, destDirName + Path.GetFileName(dir));
                }
            }
            catch (Exception ex)
            {
                throw new Exception("复制文件错误");
            }
        }

        private void W_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {

            if (e.LeftButton == MouseButtonState.Pressed)
            {
                this.DragMove();
            }

        }

        private void Btn_update_Click(object sender, RoutedEventArgs e)
        {
            this.ProgressBar.Visibility = Visibility.Visible;
            Process[] processes = Process.GetProcessesByName(this.callExeName);

            if (processes.Length > 0)
            {
                foreach (var p in processes)
                {
                    p.Kill();
                }
            }
            DownloadUpdateFile();
        }

        private void Btn_cancle_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
