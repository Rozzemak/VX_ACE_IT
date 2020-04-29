using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Flurl.Http;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.FileProviders.Physical;
using Ookii.Dialogs.WinForms;
using SevenZipExtractor;
using VX_ACE_IT_CORE.MVC.Model.Configuration;
using VX_ACE_IT_CORE.MVC.Model.TargetApp.Interfaces;

namespace VX_ACE_IT_CORE.MVC.Model.Plugins.RPGMAKER_VX_ACE.VX_ACE_SERVICES
{
    public class TargetFileService : ITargetFileService
    {
        public async Task<VistaOpenFileDialog> PickFoldersAsync(Func<VistaOpenFileDialog, Task> showDialog, Func<VistaOpenFileDialog, Task>? afterPick = null, bool awaitTask = true)
        {
            var diag = new VistaOpenFileDialog();
            // This will happen on completely different thread, so.. after-pick Action for unpacking ?
            if (awaitTask)
                diag.FileOk += async (sender, args) => await (afterPick?.Invoke(diag) ?? Task.CompletedTask);
            else diag.FileOk += async (sender, args) =>
            {
                Task.Run(async () =>
                {
                    await Task.Delay(10000);
                    return afterPick?.Invoke(diag);
                });
            }; 
            await showDialog(diag);
            return await Task.FromResult(diag);
        }

        public async Task<bool> ExtractFileAsync(IFileInfo file)
        {
            using (var archiveFile = new ArchiveFile(File.OpenRead(file.PhysicalPath)))
            {
                var directory = new DirectoryInfo(file.IsDirectory ? file.PhysicalPath : (Path.GetDirectoryName(file.PhysicalPath)));
                archiveFile.Extract(directory.FullName); 
            }
            return await Task.FromResult(true);
        } 
        
        public Task<IFileInfo> GetTargetFileAsync(string pathToTarget)
        {
            if(File.Exists(pathToTarget)) return Task.FromResult<IFileInfo>(new PhysicalFileInfo(new FileInfo(pathToTarget)));
            throw new FileNotFoundException($"File: [{pathToTarget}] not found");
        }

        public Task<bool> SearchForRelativeFileAsync(IFileInfo targetFile, string fileName, bool recursive = false)
        {
            var directory = new DirectoryInfo(targetFile.IsDirectory ? targetFile.PhysicalPath : (Path.GetDirectoryName(targetFile.PhysicalPath)));
            if (directory.GetFiles().Any(info => info.Name.Equals(fileName))) return Task.FromResult(true);
            if (!recursive) return Task.FromResult(false);
            var files = new List<FileInfo>();
            RecursiveFileSearchAsync(directory, ref files);
            files = (List<FileInfo>) files.Distinct();
            return Task.FromResult(files.Any());
        }

        public async Task<bool> DownloadFileAsync(Uri fileUrl, string? localPath)
        {
            var result = await fileUrl.DownloadFileAsync(localPath);
            return !result.Equals(string.Empty);
        }
        
        /// <summary>
        /// Travel down the directory tree to find required file.
        /// </summary>
        /// <param name="dir"></param>
        /// <param name="files"></param>
        /// <returns></returns>
        private static void RecursiveFileSearchAsync(DirectoryInfo dir, ref List<FileInfo> files)
        {
            foreach (var directoryInfo in dir.GetDirectories())
            {
                var dirs = directoryInfo.GetDirectories();
                if (!dirs.Any()) continue;
                foreach (var info in dirs)
                {
                    RecursiveFileSearchAsync(info, ref files);
                }
            }
            files.AddRange(dir.GetFiles().Intersect(files));
        }
        
    }
}