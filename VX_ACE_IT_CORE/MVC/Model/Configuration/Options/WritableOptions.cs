using System;
using System.Collections;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using VX_ACE_IT_CORE.MVC.Model.Configuration.Options.Interfaces;

namespace VX_ACE_IT_CORE.MVC.Model.Configuration.Options
{
    public class WritableOptions<T> : IWritableOptions<T> where T : class, new()
    {
        private readonly IHostingEnvironment _environment;
        private readonly IOptionsMonitor<T> _options;
        private readonly string _section;
        private readonly string _file;

        public WritableOptions(
            IHostingEnvironment environment,
            IOptionsMonitor<T> options,
            string section,
            string file)
        {
            _environment = environment;
            _options = options;
            _section = section;
            _file = file;
        }

        public T Value => _options.CurrentValue;
        public T Get(string name) => _options.Get(name);

        public void Update(Action<T> applyChanges)
        {
            var (jObject, physicalPath) = UpdateInternal(applyChanges);
            var obj = JObject.Parse(JsonConvert.SerializeObject(jObject));
            var parent = new JObject(new JProperty(_section, obj));
            File.WriteAllText(physicalPath, JsonConvert.SerializeObject(parent, Formatting.Indented));
        }

        public Task UpdateAsync(Action<T> applyChanges)
        {
            var (jObject, physicalPath) = UpdateInternal(applyChanges);
            var obj = JObject.Parse(JsonConvert.SerializeObject(jObject));
            var parent = new JObject(new JProperty(_section, obj));
            return File.WriteAllTextAsync(physicalPath, JsonConvert.SerializeObject(parent, Formatting.Indented));
        }
        
        /// <summary>
        /// Updates options file based on action with current config. 
        /// </summary>
        /// <param name="applyChanges">Func that changes object to be serialized into file</param>
        /// <returns>Json that contains changed data from original object</returns>
        private (T? jObject, string physicalPath) UpdateInternal(Action<T> applyChanges)
        {
            var settings = new JsonSerializerSettings
            {
                Formatting = Formatting.Indented,
                MissingMemberHandling = MissingMemberHandling.Error,
            };
            var fileProvider = _environment.ContentRootFileProvider;
            var fileInfo = fileProvider.GetFileInfo(_file);
            var physicalPath = fileInfo.PhysicalPath;
            // todo: make this async.
            var file = File.ReadAllText(physicalPath);
            var jObjectTemp = JsonConvert.DeserializeObject<JObject>(file, settings);
            var jObject = JsonConvert.DeserializeObject<T>(JsonConvert.SerializeObject(jObjectTemp![_section]), settings);
            applyChanges(jObject!);
            return (jObject, physicalPath);
        }
    }
}