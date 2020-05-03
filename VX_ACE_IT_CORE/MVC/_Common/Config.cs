using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Mime;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using VX_ACE_IT_CORE.Debug;
using VX_ACE_IT_CORE.MVC.Model.Async;
using VX_ACE_IT_CORE.MVC.Model.Configuration.Options;


namespace VX_ACE_IT_CORE.MVC._Common
{
    public class Config : BaseAsync<object>
    {
        private static IConfiguration? _cfg;
        public IConfiguration Configuration => GetConfig();
        private static IConfiguration GetConfig()
        {
            if (!(_cfg is null)) return _cfg;
            IConfiguration cfg;
            var devPath = Path.Combine(new DirectoryInfo(Environment.CurrentDirectory)?.Parent?.FullName,
                "API_Context");
            if (Directory.Exists(devPath))
            {
                var pth = new DirectoryInfo(Environment.CurrentDirectory).FullName + "\\appsettings.json";
                File.WriteAllText(pth, SetConfigurationWithPhrases(File.ReadAllText(pth)));
                cfg = new ConfigurationBuilder()
                    .SetBasePath(
                        $"{Path.Combine(new DirectoryInfo(Environment.CurrentDirectory)?.Parent?.FullName, "VX_ACE_IT_CORE")}")
                    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                    .AddEnvironmentVariables()
                    .Build();
            }
            else
            {
                var pth = new DirectoryInfo(Environment.CurrentDirectory)?.FullName + "\\appsettings.json";
                File.WriteAllText(pth, SetConfigurationWithPhrases( File.ReadAllText(pth)));
                cfg = new ConfigurationBuilder()
                    .SetBasePath($"{new DirectoryInfo(Environment.CurrentDirectory)?.FullName}")
                    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                    .AddEnvironmentVariables()
                    .Build();
            }
            _cfg = cfg;
            return _cfg;
        }
        
        public Config(BaseDebug debug) : base(debug, null)
        {
            var tsk = new Task<List<object>>(() =>
            {
                if (File.Exists("appsettings.json") && CheckConfigIntegrityAsync())
                {
                    //todo: Check if GetSection serialized types are readonly and refs to the actual config.
                   
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
            var tsk = new Task<List<object>>( () =>
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
            // ReSharper disable once AsyncConverter.AsyncWait
            tsk.Wait(-1);
            return (bool?) ((ResultHandler(tsk)).FirstOrDefault()) ?? false;
        }

        private static string SetConfigurationWithPhrases(string configJson)
        {
            var phrases = Regex.Matches(configJson, 
                    "{{\\w+}}", RegexOptions.CultureInvariant)
                .OfType<Match>().Select(match => match.Groups.FirstOrDefault()?.Value).ToList();
            var fields = new JsonFieldsCollector(((JToken)configJson).Values().First()).GetAllFields().ToList();
            var token = (JToken) configJson;
            Parallel.ForEach(phrases, new ParallelOptions(){MaxDegreeOfParallelism = 3},
                s =>
                {
                    s = s?.TrimStart('{');
                    s = s?.TrimEnd('}');
                    foreach (var (key, value) in fields.ToList())
                    {
                        if(!(value?.Value is null) && value.Value.ToString().Equals(s)) token[key] = 
                            FindParentWithField(token[key]!.Parent!, value.ToString(CultureInfo.InvariantCulture));
                    } 
                });
            return configJson;
        }

        private static string FindParentWithField(JToken token, string key)
        {
            key = key.TrimStart('{');
            key = key.TrimEnd('}');
            while (true)
            {
                if (!(token?[key] is null)) return token[key]!.ToString();
                token = token?.Parent!;
            }
        }
    }
}