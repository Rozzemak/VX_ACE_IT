﻿using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Flurl.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.FileProviders.Physical;
using VX_ACE_IT_CORE.MVC.Model.Configuration;
using VX_ACE_IT_CORE.MVC.Model.Configuration.Options.Interfaces;
using VX_ACE_IT_CORE.MVC.Model.TargetApp.Interfaces;

namespace VX_ACE_IT_CORE.MVC.Model.Plugins.RPGMAKER_VX_ACE.VX_ACE_SERVICES
{
    public class TargetUnpackerService : ITargetUnpackerService
    {
        private readonly IWritableOptions<GlobalConfigCfg> _configuration;
        private readonly ITargetFileService _fileService;
        private PluginsCfg PluginsCfg => _configuration.Value.PluginsCfg;
        private PluginCfg PluginCfg => PluginsCfg.Plugins.FirstOrDefault(cfg => cfg.Name.Equals("VX_ACE"));
        
        public TargetUnpackerService(IWritableOptions<GlobalConfigCfg> configuration, IServiceProvider serviceProvider)
        {
            _configuration = configuration;
            _fileService = serviceProvider.GetService<ITargetFileService>();
            // ReSharper disable once AsyncConverter.AsyncWait
            Task.Run( () => DownloadUnpackerAsync("RPGMakerDecrypter_1.0")).Wait();
            ExtractUnpackerAsync("RPGMakerDecrypter_1.0");
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

        public async Task<bool> DownloadUnpackerAsync(Uri? unpacker, string? unpackerName, string? localPath, bool useDefaultPath = true)
        {
            var result = await _fileService.DownloadFileAsync(unpacker!, (!useDefaultPath ? localPath : null ) ?? PluginsCfg.DefaultPath + "\\" + PluginsCfg.DefaultUnpackerPath + "\\" + PluginCfg.Name + "\\" + 
                                                          PluginCfg.UnpackersCfg.FirstOrDefault(cfg => cfg.Name.Equals(unpackerName))?.Name);
            return result;
        }
        
        public async Task<bool> DownloadUnpackerAsync(string unpackerName)
        {
            var unpacker = PluginCfg.UnpackersCfg.FirstOrDefault(cfg => cfg.Name.Equals(unpackerName));
            var result = !(unpacker is null) && await _fileService.DownloadFileAsync(unpacker.Uri, PluginsCfg.DefaultPath + "\\" + PluginsCfg.DefaultUnpackerPath + "\\" + PluginCfg.Name + "\\" + 
                                                                                                   unpacker.Name);
            return result;
        }

        public async Task<bool> ExtractUnpackerAsync(string unpackerName)
        {
            var file = new DirectoryInfo(PluginsCfg.DefaultPath + "\\" + PluginsCfg.DefaultUnpackerPath + "\\" +
                                         PluginCfg.Name + "\\" +
                                         PluginCfg.UnpackersCfg.FirstOrDefault(cfg =>
                                             cfg.Name.Equals(unpackerName))?.Name)?.GetFiles()?.FirstOrDefault();
            return !(file is null) && await _fileService.ExtractFileAsync(new PhysicalFileInfo(file)); 
        }

        public async Task<bool> DownloadDefaultUnpackersAsync()
        {
            var results = PluginCfg.UnpackersCfg.Select(unpacker =>
                unpacker.Uri.DownloadFileAsync(PluginsCfg.DefaultPath + "\\" + PluginsCfg.DefaultUnpackerPath + "\\" + PluginCfg.Name + "\\" + unpacker.Name));
            var result = await Task.WhenAll(results);
            return !result.Any(s => s.Equals(string.Empty));
        }

        public Task<bool> IsUnpackerLocalAsync(IFileInfo unpackerFile)
        {
            //todo: do check for custom path etc..
            return Task.FromResult(PluginCfg.UnpackersCfg.
                Select(unpacker => File.Exists($"Unpackers\\{unpacker.Name}\\{unpackerFile.Name}")).Any());
        }
    }
}