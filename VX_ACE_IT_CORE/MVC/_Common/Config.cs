using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net.Mime;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;
using Microsoft.Extensions.Configuration;
using VX_ACE_IT_CORE.Debug;
using VX_ACE_IT_CORE.MVC.Model.Async;


namespace VX_ACE_IT_CORE.MVC._Common
{
    public class Config : BaseAsync<object>
    {
        private static IConfiguration? Cfg;
        public IConfiguration Configuration => GetConfig();
        private static IConfiguration GetConfig()
        {
            if (!(Cfg is null)) return Cfg;
            IConfiguration cfg;
            var devPath = Path.Combine(new DirectoryInfo(Environment.CurrentDirectory)?.Parent?.FullName,
                "API_Context");
            if (Directory.Exists(devPath))
                cfg = new ConfigurationBuilder()
                    .SetBasePath(
                        $"{Path.Combine(new DirectoryInfo(Environment.CurrentDirectory)?.Parent?.FullName, "VX_ACE_IT_CORE")}")
                    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                    .AddEnvironmentVariables()
                    .Build();
            else
            {
                cfg = new ConfigurationBuilder()
                    .SetBasePath($"{new DirectoryInfo(Environment.CurrentDirectory)?.FullName}")
                    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                    .AddEnvironmentVariables()
                    .Build();
            }
            Cfg = cfg;
            return Cfg;
        }
        
        public Config(BaseDebug debug) : base(debug, null)
        {
            var tsk = new Task<List<object>>(() =>
            {
                if (File.Exists("appsettings.json") && CheckConfigIntegrityAsync())
                {
                    //todo: Check if GetSection serialized types are readonly and refs to the actual config.
                    Configuration["App:IsInitial"] = "false";
                }
                else
                {
                    //todo: fix this shit
                    //ReplaceJsonConfig();
                }
                return null!;
            });
            AddWork(tsk);
            tsk.ConfigureAwait(false);
        }
        
        public bool CheckConfigIntegrityAsync()
        {
            var tsk = new Task<List<object>>(() =>
            {

                lock (Configuration)
                {
                    /*
                    _xmlReader = XmlReader.Create(ConfigFileName);
                    try
                    {
                        var b = xmlSerializer.CanDeserialize(_xmlReader);
                        _xmlReader.Dispose();
                        return new List<object>() { b };
                    }
                    catch (XmlException e)
                    {
                        //todo: show error
                        //MessageBox.Show("Config.xml file is damaged. Remove it / Save new one");
                        _xmlReader.Dispose();
                        Debug.AddMessage<object>(new Message<object>("[" + typeof(XmlReader).Name + "]" + " Remove your config.xml file! => " + e.Message,MessageTypeEnum.Exception));
                        throw;
                    }
                    */
                    return new List<object>();
                }
            });
            AddWork(tsk);
            tsk.Wait(-1);
            return (bool?) ((ResultHandler(tsk)).FirstOrDefault()) ?? false;
        }

    }
}