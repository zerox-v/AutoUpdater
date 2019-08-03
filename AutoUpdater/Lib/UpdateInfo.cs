using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace AutoUpdater
{
    /// <summary>
    /// 升级信息的具体包装
    /// </summary>
    [Serializable]
    public class UpdateInfo
    {
        public string AppName { get; set; }

        /// <summary>
        /// 应用程序版本
        /// </summary>
        public Version AppVersion { get; set; }

        /// <summary>
        /// 升级需要的最低版本
        /// </summary>
        public Version RequiredMinVersion { get; set; }

        public Guid MD5
        {
            get;
            set;
        }

        private string _desc;
        /// <summary>
        /// 更新描述
        /// </summary>
        public string Desc
        {
            get
            {
                return _desc;
            }
            set
            {
                _desc = string.Join(Environment.NewLine, value.Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries));
            }
        }
    }
}
