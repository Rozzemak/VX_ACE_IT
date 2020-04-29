using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.FileProviders;
using Ookii.Dialogs.WinForms;

namespace VX_ACE_IT_CORE.MVC.Model.TargetApp.Interfaces
{
    public interface ITargetFileService
    {
        public Task<VistaOpenFileDialog> PickFoldersAsync(Func<VistaOpenFileDialog, Task> showDialog,
            Func<VistaOpenFileDialog, Task>? afterPick = null, bool awaitTask = true);
        
        public Task<bool> ExtractFileAsync(IFileInfo file);
        Task<IFileInfo> GetTargetFileAsync(string pathToTarget);

        Task<bool> SearchForRelativeFileAsync(IFileInfo targetFile, string fileName, bool recursive = false);

        public Task<bool> DownloadFileAsync(Uri fileUrl, string? localPath);
        
    }
}