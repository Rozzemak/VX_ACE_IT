using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace VX_ACE_IT_CORE.MVC.Model.Plugins.Interfaces
{
    public interface IUpdatableType
    {
        public Task Init(Dictionary<string, List<List<IntPtr>>> offsets, IEnumerable<string>? props);

        public void Init(Dictionary<string, List<List<IntPtr>>> offsets, IEnumerable<string> props, PluginBase module);

        public void BeginUpdatePrimitives(PluginBase pluginBase);

        public void SetValue<TP>(string fieldName, TP t) where TP : struct;

        public string ToDebugString(IDictionary<object, List<List<IntPtr>>> dictionary);

        public void WriteLoadedOffsets();

        public string WriteInpPtrList(List<List<IntPtr>> lists, string name);
    }
}