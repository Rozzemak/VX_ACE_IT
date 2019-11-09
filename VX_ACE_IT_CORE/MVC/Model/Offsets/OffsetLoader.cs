using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.ComponentModel;
using System.Dynamic;
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

        /// <summary>
        /// OffsetLoader for loading offsets based on defined/undefined types.
        /// You can define type that has been (dumped / set up) by config/class file. 
        /// </summary>
        /// <param name="debug">debug</param>
        /// <param name="processMethods">Methods for accessing process</param>
        /// <param name="plugin">Defined plugin suited for set engine</param>
        /// <param name="precision">Async precision.</param>
        /// <param name="objName">Has to be defined literal name of config file if type is dynamic.</param>
        /// <param name="props">Props of expando obj. Fields & methods</param>
        public OffsetLoader(BaseDebug debug, ProcessMethods processMethods, PluginBase plugin, int precision = 33, string objName = "", IEnumerable<string> props = null)
        : base(debug, processMethods._gameProcess, precision)
        {
            this._plugin = plugin;
            if (objName == "")
            {
                Type = (T)Activator.CreateInstance(typeof(T));
                if (Type.Equals(default)) return;
                Updatable = new UpdatableType<T>(debug, processMethods, Type);
                InitOffsetsAsync().ContinueWith(async task =>
                {
                    Updatable.Init(await task.ConfigureAwait(false), props, plugin);
                    Updatable.WriteLoadedOffsets();
                });
            }
            else
            {
                Type = (T)Activator.CreateInstance(typeof(T));
                // Dynamically create type of undefined one. ... Use Loader with (dynamic/object??)   
                Updatable = new UpdatableType<T>(debug, processMethods, Type);
                InitOffsetsAsync(objName, props).ContinueWith(async task =>
                {
                    Updatable.Init(await task.ConfigureAwait(false), props, plugin);
                    Updatable.WriteLoadedOffsets();
                });

            }
        }

        private async Task<Dictionary<string, List<List<IntPtr>>>> InitOffsetsAsync()
        {
            var tolerances = new Dictionary<string, (int, int)>();
            var task = new Task<List<object>>(() =>
            {
                var path = Directory.GetCurrentDirectory()
                           + "/Offsets/"
                           + _plugin.GetType().Name.Substring(0, _plugin.GetType().Name.Length)
                           + "/" + typeof(T).Name + ".xml";
                var xmlSerializer = new XmlSerializer(typeof(T));
                var offsets = new Dictionary<string, List<List<IntPtr>>>();
                var val = IntPtr.Zero;
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
                    Debug.AddMessage<object>(new Message<object>("Offsets loading from file as defined type" +
                                                                 "\nPath of UpdatableType file: \n[" + Type.GetType().Name + "]=>[" + path + "]"));
                    foreach (var field in typeof(T).GetFields())
                    {
                        var addresses = new List<IntPtr>();
                        // offsetlist.InsertRange(0, reader.Element(field.Name)?.Attributes() .Cast<IntPtr>() ?? throw new InvalidOperationException());
                        if (reader.Root?.Element(field.Name) is null ||
                            (!(bool) reader?.Root?.Element(field?.Name)?.HasAttributes)) continue;
                        offsetlists.Clear();
                        foreach (var list in reader?.Root?.Element(field?.Name)?.Attributes())
                        {
                            if (list.Name.LocalName.ToLower().Contains("tolerance") && list.Value != " ")
                            {
                                if (tolerances.ContainsKey(field.Name)) tolerances.Remove(field.Name);
                                tolerances.Add(field.Name, ((int)new Int32Converter().
                                        ConvertFromString(list.Value.Split(' ').FirstOrDefault()),
                                    ((int)new Int32Converter().
                                        ConvertFromString(list.Value.Split(' ').LastOrDefault()))));
                            }
                            foreach (var offset in list.Value.Split(' '))
                            {
                                if (!list.Name.LocalName.ToLower().Contains("tolerance") && offset != " ")
                                {
                                    if (offset.Length > 0)
                                    {
                                        val = new IntPtr(
                                            (uint)new UInt32Converter()
                                                .ConvertFromString(offset));
                                    }
                                    // Reading of 0x0 value -> adress in memory is actually required functionality.
                                    if (val != IntPtr.Zero || offset == "0x0")
                                        addresses.Add(val);
                                }
                                val = IntPtr.Zero;
                            }
                            if (addresses.Count != 0)
                            {
                                if (offsets.ContainsKey(field.Name))
                                {
                                    offsets.TryGetValue(field.Name, out var list2);
                                    list2?.Add(new List<IntPtr>(addresses));
                                }
                                else
                                {
                                    offsetlists.Add(new List<IntPtr>(addresses));
                                    offsets.Add(field.Name, new List<List<IntPtr>>(offsetlists));
                                }
                            }
                            addresses.Clear();
                        }
                    }
                }
                return new List<object>() { offsets };
            });

            AddWork(task);

            await task.ConfigureAwait(false);

            Updatable.ToleranceDict = tolerances;
            // ReSharper disable once AsyncConverter.AsyncWait
            return task.Result.FirstOrDefault() as Dictionary<string, List<List<IntPtr>>>;
        }

        private async Task<Dictionary<string, List<List<IntPtr>>>> InitOffsetsAsync(string objName, IEnumerable<string> props)
        {
            var tolerances = new Dictionary<string, (int, int)>();
            var task = new Task<List<object>>(() =>
            {
                var path = Directory.GetCurrentDirectory()
                           + "/Offsets/"
                           + _plugin.GetType().Name.Substring(0, _plugin.GetType().Name.Length)
                           + "/" + objName + ".xml";
                var xmlSerializer = new JsonFx.Xml.XmlWriter();
                var offsets = new Dictionary<string, List<List<IntPtr>>>();
                var val = IntPtr.Zero;
                if (!File.Exists(path))
                {
                    Directory.CreateDirectory(Directory.GetCurrentDirectory()
                                              + "/Offsets/");
                    Directory.CreateDirectory(Directory.GetCurrentDirectory()
                                              + "/Offsets/"
                                              + _plugin.GetType().Name.Substring(0, _plugin.GetType().Name.Length) +
                                              "/");
                    var file = File.Create(path);
                    // Low lvl streams. todo: fixme
                    xmlSerializer.Write(Type, new StreamWriter(new MemoryStream()));
                    file.Close();
                }
                else
                {
                    try
                    {
                        var offsetlists = new List<List<IntPtr>>();
                        var reader = XDocument.Load(path);
                        Debug.AddMessage<object>(new Message<object>(
                            "Offsets loading from file as undefined generic type" +
                            "\nPath of UpdatableType file: \n[" + objName + "]=>[" + path + "]"));
                        if (props.Count() != 0)
                        {
                            foreach (var field in props)
                            {
                                var adresses = new List<IntPtr>();
                                // offsetlist.InsertRange(0, reader.Element(field.Name)?.Attributes() .Cast<IntPtr>() ?? throw new InvalidOperationException());
                                if (!(reader.Root.Element(field) is null) && reader.Root.Element(field).HasAttributes)
                                {
                                    offsetlists.Clear();
                                    foreach (var list in reader.Root.Element(field)?.Attributes())
                                    {
                                        if (list.Name.LocalName.ToLower().Contains("tolerance") && list.Value != " ")
                                        {
                                            if (tolerances.ContainsKey(field)) tolerances.Remove(field);
                                            tolerances.Add(field,
                                                ((int)new Int32Converter().ConvertFromString(list.Value.Split(' ').FirstOrDefault()),
                                                    ((int)new Int32Converter().ConvertFromString(
                                                        list.Value.Split(' ').LastOrDefault()))));
                                        }

                                        foreach (var offset in list.Value.Split(' '))
                                        {
                                            if (!list.Name.LocalName.ToLower().Contains("tolerance") && offset != " ")
                                            {
                                                if (offset.Length > 0)
                                                {
                                                    val = new IntPtr(
                                                        (uint)new UInt32Converter()
                                                            .ConvertFromString(offset));
                                                }

                                                // Reading of 0x0 value -> adress in memory is actually required functionality.
                                                if (val != IntPtr.Zero || offset == "0x0")
                                                    adresses.Add(val);
                                            }

                                            val = IntPtr.Zero;
                                        }

                                        if (adresses.Count != 0)
                                        {
                                            if (offsets.ContainsKey(field))
                                            {
                                                offsets.TryGetValue(field, out var list2);
                                                list2?.Add(new List<IntPtr>(adresses));
                                            }
                                            else
                                            {
                                                offsetlists.Add(new List<IntPtr>(adresses));
                                                offsets.Add(field, new List<List<IntPtr>>(offsetlists));
                                            }
                                        }

                                        adresses.Clear();
                                    }
                                }
                            }
                        }
                        else
                        {
                            foreach (var attr in reader.Root.Elements())
                            {
                                var field = attr.Name.LocalName;
                                var adresses = new List<IntPtr>();
                                // offsetlist.InsertRange(0, reader.Element(field.Name)?.Attributes() .Cast<IntPtr>() ?? throw new InvalidOperationException());
                                if (!(reader.Root.Element(field) is null) &&
                                    reader.Root.Element(field).HasAttributes)
                                {
                                    offsetlists.Clear();
                                    foreach (var list in reader.Root.Element(field)?.Attributes())
                                    {
                                        if (list.Name.LocalName.ToLower().Contains("tolerance") &&
                                            list.Value != " ")
                                        {
                                            if (tolerances.ContainsKey(field)) tolerances.Remove(field);
                                            tolerances.Add(field,
                                                ((int)new Int32Converter().ConvertFromString(list.Value.Split(' ').FirstOrDefault()),
                                                    ((int)new Int32Converter()
                                                        .ConvertFromString(
                                                            list.Value.Split(' ').LastOrDefault()))));
                                        }

                                        foreach (var offset in list.Value.Split(' '))
                                        {
                                            if (!list.Name.LocalName.ToLower().Contains("tolerance") &&
                                                offset != " ")
                                            {
                                                if (offset.Length > 0)
                                                {
                                                    val = new IntPtr(
                                                        (uint)new UInt32Converter()
                                                            .ConvertFromString(offset));
                                                }

                                                // Reading of 0x0 value -> adress in memory is actually required functionality.
                                                if (val != IntPtr.Zero || offset == "0x0")
                                                    adresses.Add(val);
                                            }

                                            val = IntPtr.Zero;
                                        }

                                        if (adresses.Count != 0)
                                        {
                                            if (offsets.ContainsKey(field))
                                            {
                                                offsets.TryGetValue(field, out var list2);
                                                list2?.Add(new List<IntPtr>(adresses));
                                            }
                                            else
                                            {
                                                offsetlists.Add(new List<IntPtr>(adresses));
                                                offsets.Add(field, new List<List<IntPtr>>(offsetlists));
                                            }
                                        }

                                        adresses.Clear();
                                    }
                                }
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        Debug.AddMessage<object>(new Message<object>("Xml file is damaged. Remove it manually.\nException Message: {" + e.Message + "}", MessageTypeEnum.Exception));
                    }
                }
                return new List<object>() { offsets };
            });

            AddWork(task);

            return (await  task.ConfigureAwait(false))?.FirstOrDefault() as Dictionary<string, List<List<IntPtr>>>;
        }


    }
}
