using Hardcodet.Wpf.TaskbarNotification;
using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace NbDevTools
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private TaskbarIcon notifyIcon;

        public MainWindow()
        {
            InitializeComponent();
            SetupNotifyIcon();
            this.Closing += MainWindow_Closing;
        }

        private void SetupNotifyIcon()
        {
            //Bitmap bitmap = new Bitmap("icon.png");
            //Icon icon = BitmapToIcon(bitmap);
            notifyIcon = new TaskbarIcon();
            //notifyIcon.Icon = icon;
            notifyIcon.ToolTipText = "WPF App";
            notifyIcon.TrayMouseDoubleClick += NotifyIcon_TrayMouseDoubleClick;
        }

        private void NotifyIcon_TrayMouseDoubleClick(object sender, RoutedEventArgs e)
        {
            this.Show();
            this.WindowState = WindowState.Normal;
        }

        private void MainWindow_Closing(object? sender, System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = true;
            this.Hide();
            notifyIcon.ShowBalloonTip("WPF App", "The app has been minimized to the system tray", BalloonIcon.None);
        }

        public static Icon BitmapToIcon(Bitmap bitmap)
        {
            using (MemoryStream memoryStream = new MemoryStream())
            {
                bitmap.Save(memoryStream, ImageFormat.Png);
                using (Icon icon = new Icon(memoryStream))
                {
                    return (Icon)System.Drawing.Icon.FromHandle(icon.Handle).Clone();
                }
            }
        }
    }
}
