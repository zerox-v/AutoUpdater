using System;
using System.Diagnostics;
using System.IO;
using System.Windows;

namespace AutoUpdater.UI
{
   
    public partial class AlertWin : Window
    {
        public delegate void YesBtnEvent();
        public YesBtnEvent YesBtnEventCallBack;
        public AlertWin(string msg)
        {
            InitializeComponent();
            this.txtMsg.Text = msg;
        }

        private void Btn_update_Click(object sender, RoutedEventArgs e)
        {
            YesBtnEventCallBack?.Invoke();
            this.Close();
        }

        private void Btn_cancle_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
