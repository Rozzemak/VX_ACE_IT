using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.FileProviders;
using VX_ACE_IT_CORE.MVC.Model.Plugins.Interfaces;

namespace VX_ACE_IT_CORE.MVC.Model.TargetApp.Interfaces
{
    public interface ITargetVersionCheckService
    {
        Task<bool> IsVersionCompatibleAsync(IFileInfo targetFile);

        Task<IEnumerable<string>> GetCompatibleFeaturesAsync(IPluginBase pluginBase);


    }
}