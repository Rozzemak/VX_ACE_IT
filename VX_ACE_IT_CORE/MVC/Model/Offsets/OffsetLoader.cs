using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;
using VX_ACE_IT_CORE.Debug;
using VX_ACE_IT_CORE.MVC.Model.Async;
using VX_ACE_IT_CORE.MVC.Model.GameProcess;
using VX_ACE_IT_CORE.MVC.Model.Plugins;
using VX_ACE_IT_CORE.MVC.Model.Plugins.RPGMAKER_VX_ACE;

namespace VX_ACE_IT_CORE.MVC.Model.Offsets
{
    public class OffsetLoader<T> : BaseAsync<object>
    {
        public T Type;
        public UpdatableType<T> Updatable;
        private readonly PluginBase _plugin;

        public OffsetLoader(BaseDebug debug, ProcessMethods processMethods, PluginBase plugin, int precision = 33, bool defined = true)
        : base(debug, processMethods._gameProcess, precision)
        {
            this._plugin = plugin;
            if (defined)
            {
                Type = (T)Activator.CreateInstance(typeof(T));
                if (!(Type.Equals(default(object)))) // At least something defined was created.
                {
                    Updatable = new UpdatableType<T>(debug, processMethods, Type, InitOffsets(out var tolerances));
                    Updatable.ToleranceDict = tolerances;
                }
            }
            else
            {
                // Dynamically create type of undefined one. ... Use Loader with (dynamic/object??)   
            }

        }

        Dictionary<string, List<List<IntPtr>>> InitOffsets(out Dictionary<string, (int, int)> tolerances)
        {
            tolerances = new Dictionary<string, (int, int)>();
            Dictionary<string, (int, int)> tuples = tolerances;
            var task = new Task<List<object>>(() =>
            {
                var path = Directory.GetCurrentDirectory()
                           + "/Offsets/"
                           + _plugin.GetType().Name.Substring(0, _plugin.GetType().Name.Length)
                           + "/" + typeof(T).Name + ".xml";
                var xmlSerializer = new XmlSerializer(typeof(T));
                Dictionary<string, List<List<IntPtr>>> offsets = new Dictionary<string, List<List<IntPtr>>>();
                IntPtr val = IntPtr.Zero;
                if (!File.Exists(path))
                {
                    Directory.CreateDirectory(Directory.GetCurrentDirectory()
                                              + "/Offsets/");
                    Directory.CreateDirectory(Directory.GetCurrentDirectory()
                                              + "/Offsets/"
                                              + _plugin.GetType().Name.Substring(0, _plugin.GetType().Name.Length) +
                                              "/");
                    var file = File.Create(path);
                    xmlSerializer.Serialize(file, Type);
                    file.Close();
                }
                else
                {
                    var offsetlists = new List<List<IntPtr>>();
                    var reader = XDocument.Load(path);
                    Debug.AddMessage<object>(new Message<object>("Offsets loading from file"));
                    foreach (var field in typeof(T).GetFields())
                    {
                        var adresses = new List<IntPtr>();
                        // offsetlist.InsertRange(0, reader.Element(field.Name)?.Attributes() .Cast<IntPtr>() ?? throw new InvalidOperationException());
                        if (!(reader.Root.Element(field.Name) is null) && reader.Root.Element(field.Name).HasAttributes)
                        {
                            foreach (XAttribute list in reader.Root.Element(field.Name)?.Attributes())
                            {
                                if (list.Name.LocalName.ToLower().Contains("tolerance") && list.Value != " ")
                                {
                                    if (tuples.ContainsKey(field.Name)) tuples.Remove(field.Name);
                                    tuples.Add(field.Name, ((int)new System.ComponentModel.Int32Converter().
                                            ConvertFromString(list.Value.Split(' ').FirstOrDefault()),
                                        ((int)new System.ComponentModel.Int32Converter().
                                            ConvertFromString(list.Value.Split(' ').LastOrDefault()))));
                                }
                                foreach (var offset in list.Value.Split(' '))
                                {
                                    if (!list.Name.LocalName.ToLower().Contains("tolerance") && offset != " ")
                                    {
                                        if (offset != " " && offset.Length > 0)
                                        {
                                            val = new IntPtr(
                                                (uint) new System.ComponentModel.UInt32Converter()
                                                    .ConvertFromString(offset));
                                        }

                                        if (val != IntPtr.Zero)
                                            adresses.Add(val);
                                    }
                                    val = IntPtr.Zero;
                                }
                            }

                            offsets.TryGetValue(field.Name, out var offsetlists2);
                            offsetlists.Add(adresses);
                            if (offsetlists2 is null) offsets.Add(field.Name, new List<List<IntPtr>>(offsetlists));
                            else
                            {
                                offsetlists2.Add(adresses);
                            }
                        }

                        offsetlists.Clear();
                    }
                }
                return new List<object>() { offsets };
            });

            AddWork(task);

            task.Wait(-1);
            tolerances = tuples;
            return task.Result.First() as Dictionary<string, List<List<IntPtr>>>;
        }


    }
}
