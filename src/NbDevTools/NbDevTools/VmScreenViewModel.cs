using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Management.Automation;

namespace NbDevTools
{
    public class VmScreenViewModel : BindableBase
    {
        private FileInfo _droppedFileInfo;

        public DelegateCommand<object> DragEnterCommand { get; private set; }
        public DelegateCommand<object> DragOverCommand { get; private set; }
        public DelegateCommand<object> DropCommand { get; private set; }

        public VmScreenViewModel()
        {
            DragEnterCommand = new DelegateCommand<object>(OnDragEnter);
            DragOverCommand = new DelegateCommand<object>(OnDragOver);
            DropCommand = new DelegateCommand<object>(OnDrop);
        }

        public FileInfo DroppedFileInfo
        {
            get { return _droppedFileInfo; }
            set { SetProperty(ref _droppedFileInfo, value); }
        }

        private void OnDragEnter(object e)
        {
            var args = (DragEventArgs)e;
            if (args.Data.GetDataPresent(DataFormats.FileDrop))
            {
                args.Effects = DragDropEffects.Copy;
                args.Handled = true;
            }
        }

        private void OnDragOver(object e)
        {
            var args = (DragEventArgs)e;
            if (args.Data.GetDataPresent(DataFormats.FileDrop))
            {
                args.Effects = DragDropEffects.Copy;
                args.Handled = true;
            }
        }

        private void OnDrop(object sender)
        {
            var args = (DragEventArgs)sender;
            //private void Drop(object sender, DragEventArgs e)
            //{
            if (args.Data.GetDataPresent(DataFormats.FileDrop))
                {
                    string[] files = (string[])args.Data.GetData(DataFormats.FileDrop);

                    foreach (string file in files)
                    {
                        var fileInfo = new FileInfo(file);
                        string command = $"Copy-VMFile 'Hadrian_Clean' -SourcePath '{fileInfo.FullName}' -DestinationPath 'C:\\Received\\{fileInfo.Name}' -CreateFullPath -FileSource Host";
                        ExecuteCommandElevated(command);
                    }
                }
            //}

            //var args = (DragEventArgs)e;
            //if (args.Data.GetDataPresent(DataFormats.FileDrop))
            //{
            //    var files = (string[])args.Data.GetData(DataFormats.FileDrop);
            //    if (files != null && files.Length > 0)
            //    {
            //        var fileInfo = new FileInfo(files[0]);
            //        DroppedFileInfo = fileInfo;
            //        Debug.WriteLine(DroppedFileInfo.FullName);
            //        SendFileToVM(DroppedFileInfo, "Hadrian_Clean");
            //    }
            //}
        }

        private void SendFileToVM(FileInfo file, string vmName)
        {
            using (PowerShell ps = PowerShell.Create())
            {
                ps.AddCommand("ping");
                ps.AddParameter("8.8.8.8");

                ps.Invoke();

                // Load the Hyper-V module
                ps.AddCommand("Import-Module");
                ps.AddParameter("Name", "Hyper-V");

                ps.Invoke();


                // Set the command to copy the file to the VM
                ps.AddCommand("Copy-VMFile");
                ps.AddParameter("VMName", vmName);
                ps.AddParameter("FileSource", file.FullName);
                ps.AddParameter("DestinationPath", "C:\\Received\\");
                ps.AddParameter("CreateFullPath", true);

                // Execute the command
                ps.Invoke();
            }
        }

        /// <summary>
        /// We cant set <requestedExecutionLevel level="requireAdministrator" uiAccess="true" /> without changing security policy.
        /// So better off launching a new process that will request elevation as necessary
        /// </summary>
        /// <param name="command"></param>
        private void ExecuteCommandElevated(string command)
        {
            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.FileName = "powershell.exe";
            startInfo.Arguments = $"-Command \"{command}\"";
            startInfo.Verb = "runas"; // Run the process as administrator

            startInfo.UseShellExecute = true;
            startInfo.CreateNoWindow = false;
            //startInfo.RedirectStandardOutput = true; // Redirect the output stream
            //startInfo.RedirectStandardError = true; // Redirect the error stream
            Process process = Process.Start(startInfo);

            if (process == null) return;

            // Create StreamReader objects to read the output and error streams
            //StreamReader outputReader = process.StandardOutput;
            //StreamReader errorReader = process.StandardError;

            //// Read the output and error streams
            //string output = outputReader.ReadToEnd();
            //string error = errorReader.ReadToEnd();

            //Debug.WriteLine($"Output: {output}");
            //Debug.WriteLine($"Error: {error}");

            //// Wait for the process to exit
            //process.WaitForExit();
        }

    }
}
