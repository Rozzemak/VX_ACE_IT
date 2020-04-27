using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Flurl.Http;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.FileProviders.Physical;
using VX_ACE_IT_CORE.MVC.Model.Configuration;
using VX_ACE_IT_CORE.MVC.Model.TargetApp.Interfaces;
using NotImplementedException = System.NotImplementedException;

namespace VX_ACE_IT_CORE.MVC.Model.Plugins.RPGMAKER_VX_ACE.VX_ACE_SERVICES
{
    public class TargetFileService : ITargetFileService
    {
        public Task<IFileInfo> GetTargetFileAsync(string pathToTarget)
        {
            if(File.Exists(pathToTarget)) return Task.FromResult<IFileInfo>(new PhysicalFileInfo(new FileInfo(pathToTarget)));
            throw new FileNotFoundException($"File: [{pathToTarget}] not found");
        }

        public Task<bool> SearchForRelativeFileAsync(IFileInfo targetFile, string fileName, bool recursive = false)
        {
            var directory = new DirectoryInfo(targetFile.IsDirectory ? targetFile.PhysicalPath : (Path.GetPathRoot(targetFile.PhysicalPath)));
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