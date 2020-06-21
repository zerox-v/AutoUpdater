using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace AutoUpdater.Lib
{
  public static  class File
    {
        /// <summary>
        /// 检查文件
        /// </summary>
        /// <param name="path"></param>
        public static void CheckDir(string path)
        {
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
        }

        /// <summary>
        /// 写入文件
        /// </summary>
        /// <param name="path"></param>
        /// <param name="str"></param>
        public static void Write(string path, string str, FileMode fileMode = FileMode.Create)
        {
            try
            {
                FileStream fs = new FileStream(path, fileMode);
                //获得字节数组
                byte[] data = System.Text.Encoding.Default.GetBytes(str);
                //开始写入
                fs.Write(data, 0, data.Length);
                //清空缓冲区、关闭流
                fs.Flush();
                fs.Close();
            }
            catch (Exception)
            {


            }

        }

        /// <summary>
        /// 读取文件
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static string Read(string path)
        {
            if (System.IO.File.Exists(path))
            {
                StreamReader sr = new StreamReader(path, Encoding.Default);
                string result = sr.ReadToEnd();
                sr.Close();
                return result;
            }
            return "";
        }

        public static string GetRootPath()
        {
            return AppDomain.CurrentDomain.BaseDirectory;
        }
    }
}
