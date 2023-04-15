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
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows.Interop;

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
            SetupHotKey();
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






        [DllImport("user32.dll")]
        private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

        [DllImport("user32.dll")]
        private static extern bool UnregisterHotKey(IntPtr hWnd, int id);

        // This needs to increment with every "register", unless we only ever want a single hook
        private const int HOTKEY_ID = 9000;

        // See https://learn.microsoft.com/en-us/windows/win32/inputdev/virtual-key-codes for these key codes

        //Modifiers:
        private const uint MOD_NONE = 0x0000; //(none)
        private const uint MOD_ALT = 0x0001; //ALT
        private const uint MOD_CONTROL = 0x0002; //CTRL
        private const uint MOD_SHIFT = 0x0004; //SHIFT
        private const uint MOD_WIN = 0x0008; //WINDOWS

        //Keys:
        private const uint VK_RETURN = 0x0D;
        private const uint VK_CAPITAL = 0x14;

        // WM_HOTKEY is a Windows message constant that is sent to a window when the user presses a hotkey.
        const int WM_HOTKEY = 0x0312;

        // HwndMessage is set to (IntPtr)(-3), which is equivalent to the HWND_MESSAGE constant in the Windows API.
        // HWND_MESSAGE is a special constant handle value that is used to indicate that a message is
        // being sent to all top-level windows in the system, rather than a specific window.
        internal static readonly IntPtr HwndMessage = (IntPtr)(-3);

        private IntPtr _globalHandle;
        private HwndSource _source;


        public void SetupHotKey()
        {
            var parameters = new HwndSourceParameters("Hotkey sink")
            {
                HwndSourceHook = HwndHook,
                ParentWindow = HwndMessage
            };
            _source = new HwndSource(parameters);

            _globalHandle = _source.Handle;

            UnregisterHotKey(_globalHandle, HOTKEY_ID);
            bool success = RegisterHotKey(_globalHandle, HOTKEY_ID, MOD_WIN | MOD_ALT, VK_RETURN); //CTRL + CAPS_LOCK

            if (!success)
            {
                int error = Marshal.GetLastWin32Error();
                Debug.WriteLine("Failed to register hotkey. Error code: " + error.ToString());
            }

        }


        private IntPtr HwndHook(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            switch (msg)
            {
                case WM_HOTKEY:
                    switch (wParam.ToInt32())
                    {
                        case HOTKEY_ID:

                            // The lParam parameter is a 32-bit integer that contains the modifiers and the virtual key code of the hotkey that was pressed.
                            // The high-order word of the lParam parameter contains the modifiers (i.e., the key that was pressed in combination with the hotkey),
                            // and the low-order word contains the virtual key code of the hotkey itself.
                            int vkey = (((int)lParam >> 16) & 0xFFFF);

                            this.Dispatcher.Invoke(() =>
                            {
                                if(this.Visibility == Visibility.Visible && this.Topmost)
                                {
                                    this.Hide();
                                }
                                else
                                {
                                    this.Show();
                                    this.Topmost = true;
                                }

                            });

                            handled = true;
                            break;
                    }
                    break;
            }
            return IntPtr.Zero;
        }

        protected override void OnClosed(EventArgs e)
        {
            _source.RemoveHook(HwndHook);
            UnregisterHotKey(_globalHandle, HOTKEY_ID);
            base.OnClosed(e);
        }
    }
}
