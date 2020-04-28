using System;
using Microsoft.AspNetCore.Hosting.Internal;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using VX_ACE_IT_CORE.Debug;
using VX_ACE_IT_CORE.MVC.Controller;
using VX_ACE_IT_CORE.MVC._Common;
using VX_ACE_IT_CORE.MVC.Model.Async;
using VX_ACE_IT_CORE.MVC.Model.Configuration;
using VX_ACE_IT_CORE.MVC.Model.Configuration.Options;
using VX_ACE_IT_CORE.MVC.Model.Configuration.Options.Interfaces;
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
        public readonly Controller Controller;

        public Core(BaseDebug debug, Config config)
        {
            SetupServices(_services, config);
            Controller = new Controller(_services.BuildServiceProvider(), debug, config);
        }

        private void SetupServices(IServiceCollection services, Config config)
        {
            //services.Configure<GlobalConfigCfg>(config.Configuration);
            var readonlyConfig = config.Configuration.GetSection(nameof(GlobalConfigCfg)).Get<GlobalConfigCfg>();
            services.AddSingleton<IHostingEnvironment>(new HostingEnvironment(){ApplicationName = readonlyConfig.AppCfg.Name, EnvironmentName = "PC", 
                ContentRootPath = Environment.CurrentDirectory, ContentRootFileProvider = new PhysicalFileProvider(Environment.CurrentDirectory),
                WebRootPath = Environment.CurrentDirectory, WebRootFileProvider = new PhysicalFileProvider(Environment.CurrentDirectory)
            });
            services.AddOptions<GlobalConfigCfg>();
            services.ConfigureWritable<GlobalConfigCfg>(config.Configuration.GetSection(nameof(GlobalConfigCfg)));
            services.AddScoped<ITargetFileService, TargetFileService>();
            services.AddScoped<ITargetUnpackerService, TargetUnpackerService>();
            services.AddScoped<IPluginBase, VxAceModule>();
            services.AddScoped<IUpdatableType, UpdatableType<BaseAsync<object>>>();
        }

    }
    
    public static class ServiceCollectionExtensions
    {
        public static void ConfigureWritable<T>(
            this IServiceCollection services,
            IConfigurationSection section,
            string file = "appsettings.json") where T : class, new()
        {
            services.Configure<T>(section);
            services.AddTransient<IWritableOptions<T>>(provider =>
            {
                var environment = provider.GetService<IHostingEnvironment>();
                var options = provider.GetService<IOptionsMonitor<T>>();
                return new WritableOptions<T>(environment, options, section.Key, file);
            });
        }
    }
}
