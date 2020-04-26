using System;
using Microsoft.Extensions.DependencyInjection;
using VX_ACE_IT_CORE.Debug;
using VX_ACE_IT_CORE.MVC.Controller;
using VX_ACE_IT_CORE.MVC._Common;
using VX_ACE_IT_CORE.MVC.Model.Async;
using VX_ACE_IT_CORE.MVC.Model.Plugins;
using VX_ACE_IT_CORE.MVC.Model.Plugins.Interfaces;
using VX_ACE_IT_CORE.MVC.Model.Plugins.RPGMAKER_VX_ACE;
using VX_ACE_IT_CORE.MVC.Model.Plugins.RPGMAKER_VX_ACE.VX_ACE_SERVICES;
using VX_ACE_IT_CORE.MVC.Model.TargetApp.Interfaces;

namespace VX_ACE_IT_CORE
{
    public class Core
    {
        private readonly IServiceCollection _services = new ServiceCollection();
        public readonly Controller _controller;

        public Core(BaseDebug debug, Config config)
        {
            SetupServices(_services);
            _controller = new Controller(_services.BuildServiceProvider(), debug, config);
        }

        private void SetupServices(IServiceCollection services)
        {
            services.AddScoped<ITargetFileService, TargetFileService>();
            services.AddScoped<ITargetUnpackerService, TargetUnpackerService>();
            services.AddScoped<IPluginBase, VxAceModule>();
            services.AddScoped<IUpdatableType, UpdatableType<BaseAsync<object>>>();
        }

    }
}
