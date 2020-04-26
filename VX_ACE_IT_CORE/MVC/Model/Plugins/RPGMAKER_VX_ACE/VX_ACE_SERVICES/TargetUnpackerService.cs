using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Flurl.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using VX_ACE_IT_CORE.MVC.Model.Configuration;
using VX_ACE_IT_CORE.MVC.Model.TargetApp.Interfaces;

namespace VX_ACE_IT_CORE.MVC.Model.Plugins.RPGMAKER_VX_ACE.VX_ACE_SERVICES
{
    public class TargetUnpackerService : ITargetUnpackerService
    {
        private readonly IConfiguration _configuration;
        private readonly ITargetFileService _fileService;
        private PluginsCfg PluginsCfg => _configuration.GetSection(nameof(PluginsCfg)).Get<PluginsCfg>();
        private PluginCfg PluginCfg => PluginsCfg.Plugins.FirstOrDefault(cfg => cfg.Name.Equals("VX_ACE"));
        
        public TargetUnpackerService(IConfiguration configuration, IServiceProvider serviceProvider)
        {
            _configuration = configuration;
            _fileService = serviceProvider.GetService<ITargetFileService>();
        }
        public Task<bool> UnpackTargetExternalAsync(IFileInfo fileToUnpack, IFileInfo unpackerLocal, bool tryToDownloadNonLocal = true,
            string unpackerParams = "", bool forceWithoutParams = false)
        {
            throw new NotImplementedException();
        }

        public Task<bool> UnpackTargetExternalAsync(IFileInfo fileToUnpack, Uri unpackerUrl, string unpackerParams = "",
            bool forceWithoutParams = false)
        {
            throw new NotImplementedException();
        }

        public async Task<bool> DownloadUnpackerAsync(Uri unpacker, string? localPath)
        {
            var result = await unpacker.DownloadFileAsync(localPath ?? "Unpackers\\" + PluginCfg.Name + "\\" + 
                                                          PluginCfg.Unpackers.FirstOrDefault(cfg => cfg.Uri.Equals(unpacker)).Name);
            return !result.Equals(string.Empty);
        }
        
        public async Task<bool> DownloadUnpackerAsync(string unpackerName, string? localPath)
        {
            var result = await PluginCfg.Unpackers.FirstOrDefault(cfg => cfg.Name.Equals(unpackerName)).Uri
                .DownloadFileAsync(localPath ?? "Unpackers\\" + PluginCfg.Name + "\\" + 
                PluginCfg.Unpackers.FirstOrDefault(cfg => cfg.Name.Equals(unpackerName)).Name);
            return !result.Equals(string.Empty);
        }

        public async Task<bool> DownloadDefaultUnpackersAsync()
        {
            var results = PluginCfg.Unpackers.Select(unpacker =>
                unpacker.Uri.DownloadFileAsync("Unpackers\\" + PluginCfg.Name + "\\" + unpacker.Name));
            var result = await Task.WhenAll(results);
            return !result.Any(s => s.Equals(string.Empty));
        }

        public Task<bool> IsUnpackerLocalAsync(IFileInfo unpackerFile)
        {
            //todo: do check for custom path etc..
            return Task.FromResult(PluginCfg.Unpackers.
                Select(unpacker => File.Exists($"Unpackers\\{unpacker.Name}\\{unpackerFile.Name}")).Any());
        }
    }
}