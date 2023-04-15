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

        private void OnDrop(object e)
        {
            var args = (DragEventArgs)e;
            if (args.Data.GetDataPresent(DataFormats.FileDrop))
            {
                var files = (string[])args.Data.GetData(DataFormats.FileDrop);
                if (files != null && files.Length > 0)
                {
                    var fileInfo = new FileInfo(files[0]);
                    DroppedFileInfo = fileInfo;
                    Debug.WriteLine(DroppedFileInfo.FullName);
                }
            }
        }
    }
}
