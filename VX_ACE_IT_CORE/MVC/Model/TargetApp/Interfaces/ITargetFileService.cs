using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.FileProviders;

namespace VX_ACE_IT_CORE.MVC.Model.TargetApp.Interfaces
{
    public interface ITargetFileService
    {
        Task<IFileInfo> GetTargetFileAsync(string pathToTarget);

        Task<bool> SearchForRelativeFileAsync(IFileInfo targetFile, string fileName, bool recursive = false);
    }
}