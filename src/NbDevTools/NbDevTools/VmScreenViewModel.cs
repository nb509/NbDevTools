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
using System.Collections.ObjectModel;
using System.Management.Automation.Runspaces;
using System.Security;

namespace NbDevTools
{
    public class VmScreenViewModel : BindableBase
    {
        private FileInfo _droppedFileInfo;

        public ObservableCollection<FileInfo> LocalFiles { get; set; }
        public ObservableCollection<FileInfo> RemoteFiles { get; set; } = new ObservableCollection<FileInfo>();

        public VmScreenViewModel()
        {
            LocalFiles = new ObservableCollection<FileInfo>(new DirectoryInfo(@"C:\").GetFiles());

            Initialise();
        }

        private async void Initialise()
        {
            //await CreateSession("Hadrian_Clean", "Nick", " ");
            await CreateSession("169.254.179.63", "Nick", " ");

            

            // List the files
            await PopulateRemoteFiles(@"C:\Received");

            // Dispose of the session
            DisposeSession();
        }


        //private void OnDrop(object sender)
        //{
        //    var args = (DragEventArgs)sender;
        //    //private void Drop(object sender, DragEventArgs e)
        //    //{
        //    if (args.Data.GetDataPresent(DataFormats.FileDrop))
        //        {
        //            string[] files = (string[])args.Data.GetData(DataFormats.FileDrop);



        //            foreach (string file in files)
        //            {
        //                var fileInfo = new FileInfo(file);
        //                string command = $"Copy-VMFile 'Hadrian_Clean' -SourcePath '{fileInfo.FullName}' -DestinationPath 'C:\\Received\\{fileInfo.Name}' -CreateFullPath -FileSource Host";
        //                ExecuteCommandElevated(command);
        //            }
        //        }

        //}

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

                ps.ToString();

                // Execute the command
                ps.Invoke();
            }
        }

        

        //public async Task PopulateRemoteFiles()
        //{
        //    RemoteFiles.Clear();

        //    using (var ps = PowerShell.Create())
        //    {
        //        ps.AddCommand("Get-ChildItem");
        //        ps.AddParameter("Path", @"F:\Nick\temp - can delete");
        //        ps.AddParameter("File");
        //        ps.AddParameter("Recurse");

        //        ps.Invoke();

        //        ps.Runspace = RunspaceFactory.CreateRunspace();
        //        ps.Runspace.Open();

        //        var results = await Task.Factory.FromAsync(ps.BeginInvoke(), ps.EndInvoke);

        //        foreach (var item in results)
        //        {
        //            var fileInfo = new FileInfo(item.BaseObject.ToString());
        //            RemoteFiles.Add(fileInfo);
        //        }
        //    }
        //}

        private PowerShell _ps;
        private PSSession _session;


        /// <summary>
        /// Creates a remove session using the WinRM service
        /// Requires WinRM to be enabled on the target device. To do this:
        /// 1. The network adaptor must not be set to public. Use: 
        ///         a. Get-NetAdapter
        ///         b. Set-NetConnectionProfile -InterfaceIndex <index> -NetworkCategory Private
        /// 2. Allow your PC: Set-Item wsman:\localhost\client\trustedhosts *      (WARNING: this enables all machines. you may want to add your own machine name so that its not left wide open)
        /// </summary>
        /// <param name="computerName"></param>
        /// <param name="userName"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public async Task CreateSession(string computerName, string userName, string password)
        {
            // TODO: add tests!

            var securePassword = new SecureString();
            foreach (var c in password)
            {
                securePassword.AppendChar(c);
            }

            // . By default, WinRM uses HTTP on port 5985 for non-encrypted communication, and HTTPS on port 5986 for encrypted communication.
            // We can configure WinRM to use different ports, or to require encryption for all communication.
            var connectionInfo = new WSManConnectionInfo(new Uri("http://" + computerName + ":5985/wsman"), "http://schemas.microsoft.com/powershell/Microsoft.PowerShell", new PSCredential(userName, securePassword));
            _ps = PowerShell.Create();

            // Redirect PowerShell output to the WPF console output.
            _ps.Streams.Error.DataAdded += (sender, args) => Debug.WriteLine(_ps.Streams.Error[args.Index].ToString());
            _ps.Streams.Warning.DataAdded += (sender, args) => Debug.WriteLine(_ps.Streams.Warning[args.Index].ToString());
            _ps.Streams.Verbose.DataAdded += (sender, args) => Debug.WriteLine(_ps.Streams.Verbose[args.Index].ToString());
            _ps.Streams.Progress.DataAdded += (sender, args) => Debug.WriteLine(_ps.Streams.Progress[args.Index].ToString());
            _ps.Streams.Debug.DataAdded += (sender, args) => Debug.WriteLine(_ps.Streams.Debug[args.Index].ToString());
            _ps.Streams.Information.DataAdded += (sender, args) => Debug.WriteLine(_ps.Streams.Information[args.Index].ToString());
            

            // This only needs doing on the first connection ever:
            AddMachineToTrustedHosts(computerName);

            // So does this:
            SendVMConfigurationScript("Hadrian_Clean", "Ethernet"); // This needs someone to run the script on the destination VM


            _ps.AddCommand("New-PSSession");
            _ps.AddParameter("ConnectionUri", connectionInfo.ConnectionUri);
            _ps.AddParameter("Credential", connectionInfo.Credential);
            _ps.AddParameter("Authentication", "Default");
            _ps.AddParameter("Name", "RemoteSession");

            var result = await _ps.InvokeAsync();

            if (_ps.HadErrors)
            {
                var errorMessage = string.Join(Environment.NewLine, _ps.Streams.Error.Select(e => e.ToString()));
                throw new Exception($"Failed to create session on remote machine: {errorMessage}");
            }

            if (_ps.Streams.Warning.Count > 0)
            {
                var warningMessage = string.Join(Environment.NewLine, _ps.Streams.Warning.Select(w => w.ToString()));
                Console.WriteLine($"Warning while creating session on remote machine: {warningMessage}");
            }

            var psSession = result.FirstOrDefault()?.BaseObject as PSSession;

            if (psSession == null)
            {
                throw new Exception("Failed to create session on remote machine.");
            }

            _session = psSession;
        }

 
        public void AddMachineToTrustedHosts(string machineName)
        {
            _ps.AddCommand("Set-Item")
                .AddParameter("Path", "WSMan:\\localhost\\Client\\TrustedHosts")
                .AddParameter("Value", machineName)
                .AddParameter("Force")
                .Invoke();
        }

        public async Task PopulateRemoteFiles(string path)
        {
            RemoteFiles.Clear();

            var result = await _ps.AddCommand("Get-ChildItem")
                                  .AddParameter("Path", path)
                                  .AddParameter("File")
                                  .AddParameter("Recurse")
                                  .AddParameter("EnumeratorMode", "Both")
                                  .InvokeAsync();

            foreach (var item in result)
            {
                var fileInfo = new FileInfo(item.BaseObject.ToString());
                RemoteFiles.Add(fileInfo);
            }
        }

        public void DisposeSession()
        {
            if (_session != null)
            {
                _ps.Commands.Clear();
                _ps.AddCommand("Remove-PSSession").AddParameter("Session", _session).Invoke();
                _session = null;
            }
            if (_ps != null)
            {
                _ps.Dispose();
                _ps = null;
            }
        }


        private SecureString GetSecurePassword(string password)
        {
            var securePassword = new SecureString();
            foreach (var c in password)
            {
                securePassword.AppendChar(c);
            }
            return securePassword;
        }



        /// <summary>
        /// We need to do a few things to make the VM accept our remote powershell session:
        /// 1. Enable WinRM (which requires a private network adaptor)
        /// 
        /// </summary>
        /// <param name="vmName"></param>
        /// <param name="networkAdapterName"></param>
        public void SendVMConfigurationScript(string vmName, string networkAdapterName)
        {
            // Set up the PowerShell script content
            var scriptContent = $@"
                $adapter = Get-NetAdapter -Name {networkAdapterName}
                Set-NetConnectionProfile -InterfaceIndex $adapter.ifIndex -NetworkCategory Private
                Enable-PSRemoting -Force
                Set-Item wsman:\localhost\client\trustedhosts {Environment.MachineName}
                Restart-Service WinRM
                ";


            // Write the script content to a file on your local machine
            string scriptPath = @"C:\\NbDevToolsVMConfigurationScript.ps1";
            File.WriteAllText(scriptPath, scriptContent);

            _ps.Commands.Clear();

            // Copy the script file to the VM
            var result = _ps.AddCommand("Copy-VMFile")
                .AddParameter("Name", vmName)
                .AddParameter("SourcePath", scriptPath)
                .AddParameter("DestinationPath", scriptPath)
                .AddParameter("CreateFullPath", true)
                .AddParameter("FileSource", "Host")
                .AddParameter("Force", true)
                .Invoke();

            _ps.Commands.Clear();

            if (_ps.HadErrors)
            {
                var errorMessage = string.Join(Environment.NewLine, _ps.Streams.Error.Select(e => e.ToString()));
                throw new Exception($"Failed to create session on remote machine: {errorMessage}");
            }

            if (_ps.Streams.Warning.Count > 0)
            {
                var warningMessage = string.Join(Environment.NewLine, _ps.Streams.Warning.Select(w => w.ToString()));
                Console.WriteLine($"Warning while creating session on remote machine: {warningMessage}");
            }

            foreach (var item in result)
            {
                Debug.WriteLine(item.ToString());
            }


            // Remove the script file from your local machine
            File.Delete(scriptPath);
        }
    }
}
