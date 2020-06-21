using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows;


using System.Xml.Linq;

namespace AutoUpdater
{
    public class Updater
    {
        private static Updater _instance;

        public delegate void ShowUpdateWindow();

        public ShowUpdateWindow ShowUpdateWindowCallBack;

        public static Updater Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new Updater();
                }
                return _instance;
            }
        }

        public static void CheckUpdateStatus()
        {
            System.Threading.ThreadPool.QueueUserWorkItem((s) =>
            {
                string url = Constants.RemoteUrl + Updater.Instance.CallExeName + "/update.xml";
                var client = new System.Net.WebClient();
                AutoUpdater.Lib.File.Write("Log.txt", $"开始获取数据{url}\r\n", FileMode.Append);
                client.DownloadDataCompleted += (x, y) =>
                {
                    try
                    {

                        AutoUpdater.Lib.File.Write("Log.txt", $"开始读取数据\r\n", FileMode.Append);
                        MemoryStream stream = new MemoryStream(y.Result);
                        AutoUpdater.Lib.File.Write("Log.txt", $"读取数据结束\r\n", FileMode.Append);
                        AutoUpdater.Lib.File.Write("Log.txt", $"开始加载数据\r\n", FileMode.Append);
                        XDocument xDoc = XDocument.Load(stream);
                        AutoUpdater.Lib.File.Write("Log.txt", $"加载数据结束\r\n", FileMode.Append);
                        UpdateInfo updateInfo = new UpdateInfo();
                        XElement root = xDoc.Element("UpdateInfo");
                        updateInfo.AppName = root.Element("AppName").Value;
                        updateInfo.AppVersion = root.Element("AppVersion") == null || string.IsNullOrEmpty(root.Element("AppVersion").Value) ? null : new Version(root.Element("AppVersion").Value);
                        updateInfo.RequiredMinVersion = root.Element("RequiredMinVersion") == null || string.IsNullOrEmpty(root.Element("RequiredMinVersion").Value) ? null : new Version(root.Element("RequiredMinVersion").Value);
                        updateInfo.Desc = root.Element("Desc").Value;
                        updateInfo.MD5 = Guid.NewGuid();
                        AutoUpdater.Lib.File.Write("Log.txt",updateInfo.AppName+ "\r\n", FileMode.Append);

                        stream.Close();
                        Updater.Instance.StartUpdate(updateInfo);
                    }
                    catch (Exception ex) {
                        AutoUpdater.Lib.File.Write("Log.txt",ex.StackTrace, FileMode.Append);
                        MessageBox.Show(ex.Message);
                     
                    }
                    
                };
                client.DownloadDataAsync(new Uri(url));

            });

        }

        public void StartUpdate(UpdateInfo updateInfo)
        {
            if (updateInfo.RequiredMinVersion != null && Updater.Instance.CurrentVersion < updateInfo.RequiredMinVersion)
            {
                //当前版本比需要的版本小，不更新
                return;
            }

            if (Updater.Instance.CurrentVersion >= updateInfo.AppVersion)
            {
                //当前版本是最新的，不更新
                return;
            }

            //更新程序复制到缓存文件夹
            string appDir = System.IO.Path.Combine(System.Reflection.Assembly.GetEntryAssembly().Location.Substring(0, System.Reflection.Assembly.GetEntryAssembly().Location.LastIndexOf(System.IO.Path.DirectorySeparatorChar)));
            string updateFileDir = System.IO.Path.Combine(System.IO.Path.Combine(appDir.Substring(0, appDir.LastIndexOf(System.IO.Path.DirectorySeparatorChar))), "Update");
            if (!Directory.Exists(updateFileDir))
            {
                Directory.CreateDirectory(updateFileDir);
            }
            updateFileDir = System.IO.Path.Combine(updateFileDir, updateInfo.MD5.ToString());
            if (!Directory.Exists(updateFileDir))
            {
                Directory.CreateDirectory(updateFileDir);
            }

            string exePath = System.IO.Path.Combine(updateFileDir, "AutoUpdater.exe");
            File.Copy(System.IO.Path.Combine(appDir, "AutoUpdater.exe"), exePath, true);

            var info = new System.Diagnostics.ProcessStartInfo(exePath);
            info.UseShellExecute = true;
            info.WorkingDirectory = exePath.Substring(0, exePath.LastIndexOf(System.IO.Path.DirectorySeparatorChar));
            updateInfo.Desc = updateInfo.Desc;
            info.Arguments = "update " + Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(CallExeName)) + " " + Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(updateFileDir)) + " " + Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(appDir)) + " " + Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(updateInfo.AppName)) + " " + Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(updateInfo.AppVersion.ToString())) + " " + (string.IsNullOrEmpty(updateInfo.Desc) ? "" : Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(updateInfo.Desc)));
            ShowUpdateWindowCallBack?.Invoke();
            System.Diagnostics.Process.Start(info);
        }

        public bool UpdateFinished = false;

        private string _callExeName;
        public string CallExeName
        {
            get
            {
                if (string.IsNullOrEmpty(_callExeName))
                {
                    _callExeName = System.Reflection.Assembly.GetEntryAssembly().Location.Substring(System.Reflection.Assembly.GetEntryAssembly().Location.LastIndexOf(System.IO.Path.DirectorySeparatorChar) + 1).Replace(".exe", "");
                }
                return _callExeName;
            }
        }

        /// <summary>
        /// 获得当前应用软件的版本
        /// </summary>
        public virtual Version CurrentVersion
        {
            get { return new Version(System.Diagnostics.FileVersionInfo.GetVersionInfo(System.Reflection.Assembly.GetEntryAssembly().Location).ProductVersion); }
        }

        /// <summary>
        /// 获得当前应用程序的根目录
        /// </summary>
        public virtual string CurrentApplicationDirectory
        {
            get { return System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location); }
        }
    }
}
