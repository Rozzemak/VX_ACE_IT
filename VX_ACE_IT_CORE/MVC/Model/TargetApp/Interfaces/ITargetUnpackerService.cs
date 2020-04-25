using System;
using System.Threading.Tasks;
using Microsoft.Extensions.FileProviders;

namespace VX_ACE_IT_CORE.MVC.Model.TargetApp.Interfaces
{
    public interface ITargetUnpackerService
    {
        //todo: insert unpacker default params into appsettings, plugin dependable should then be visible in frontend and update by path

        /// <summary>
        /// Checks whether unpacker exists, if not downloads it from url from config, then uses cmd params from consumer or default from cfg.
        /// </summary>
        /// <param name="fileToUnpack">File to be unpacked, fetched from appsettings or set from consumer app</param>
        /// <param name="unpackerLocal">Unpacker to be used on said file</param>
        /// <param name="tryToDownloadNonLocal">Whether to download unpacker if local does not exists</param>
        /// <param name="unpackerParams">Params of provided unpacker, fetched from appsettings or set from consumer app</param>
        /// <param name="forceWithoutParams">Ignore default params in appsettings</param>
        /// <returns>Success</returns>
        Task<bool> UnpackTargetExternalAsync(IFileInfo fileToUnpack, IFileInfo unpackerLocal, bool tryToDownloadNonLocal = true, string unpackerParams = "", bool forceWithoutParams = false);
        
        /// <summary>
        /// Checks whether unpacker exists, if not downloads it from url from config, then uses cmd params from consumer or default from cfg.
        /// </summary>
        /// <param name="fileToUnpack">File to be unpacked, fetched from appsettings or set from consumer app</param>
        /// <param name="unpackerUrl">Unpacker to be downloaded and used.., uri fetched from appsettings or set via consumer app</param>
        /// <param name="unpackerParams">Params of provided unpacker, fetched from appsettings or set from consumer app</param>
        /// <param name="forceWithoutParams">Ignore default params in appsettings</param>
        /// <returns></returns>
        Task<bool> UnpackTargetExternalAsync(IFileInfo fileToUnpack, Uri unpackerUrl, string unpackerParams = "", bool forceWithoutParams = false);
        
        Task<bool> DownloadUnpackerAsync(Uri unpacker);
        
        /// <summary>
        /// Downloads all unpackers with defined Uri ands saves them locally.
        /// </summary>
        /// <returns></returns>
        Task<bool> DownloadDefaultUnpackersAsync();

        Task<bool> IsUnpackerLocalAsync(IFileInfo unpacker);


    }
}