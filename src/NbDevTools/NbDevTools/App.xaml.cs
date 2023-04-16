using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO.Pipes;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Management.Automation.Runspaces;
using System.Threading;

namespace NbDevTools
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public App()
        {
            Debug.WriteLine(Thread.CurrentThread.CurrentCulture + " " + Thread.CurrentThread.CurrentUICulture);
        }
    }
}
