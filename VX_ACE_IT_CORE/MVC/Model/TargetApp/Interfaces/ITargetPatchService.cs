using System;
using System.Threading.Tasks;
using Microsoft.Extensions.FileProviders;

namespace VX_ACE_IT_CORE.MVC.Model.TargetApp.Interfaces
{
    public interface ITargetPatchService
    {
        //todo: also add default file providers to appsettings
        /// <summary>
        /// Checks whether patcher exists, if not downloads it from url from config, then uses cmd params from consumer or default from cfg.
        /// </summary>
        /// <param name="fileToUnpack">File to be unpacked, fetched from appsettings or set from consumer app</param>
        /// <param name="patcherLocal">Patcher to be used on said file</param>
        /// <param name="tryToDownloadNonLocal">Whether to download patcher if local does not exists</param>
        /// <param name="patcherParams">Params of provided patcher, fetched from appsettings or set from consumer app</param>
        /// <param name="forceWithoutParams">Ignore default params in appsettings</param>
        /// <returns>Success</returns>
        Task<bool> PatchTargetTargetExternalAsync(IFileInfo fileToUnpack, IFileInfo patcherLocal, bool tryToDownloadNonLocal = true, string patcherParams = "", bool forceWithoutParams = false);
        
        /// <summary>
        /// Checks whether patcher exists, if not downloads it from url from config, then uses cmd params from consumer or default from cfg.
        /// </summary>
        /// <param name="fileToUnpack">File to be unpacked, fetched from appsettings or set from consumer app</param>
        /// <param name="patcherUrl">Patcher to be downloaded and used.., uri fetched from appsettings or set via consumer app</param>
        /// <param name="patcherParams">Params of provided patcher, fetched from appsettings or set from consumer app</param>
        /// <param name="forceWithoutParams">Ignore default params in appsettings</param>
        /// <returns></returns>
        Task<bool> PatchTargetExternalAsync(IFileInfo fileToUnpack, Uri patcherUrl, string patcherParams = "", bool forceWithoutParams = false);
        
        Task<bool> DownloadPatcherAsync(Uri patcher);
        
        /// <summary>
        /// Downloads all patchers with defined Uri ands saves them locally.
        /// </summary>
        /// <returns></returns>
        Task<bool> DownloadDefaultPatchersAsync();

        Task<bool> IsPatchLocalAsync(IFileInfo patcher);
        Task<bool> IsFilePatchedAsync(IFileInfo targetFile, IFileInfo filePatch);
    }
}