using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
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
                if (!(Type.Equals(default(object)))) // At least something defined was created.
                {
                    Updatable = new UpdatableType<T>(debug, processMethods, Type, InitOffsets(out var tolerances));
                    Updatable.ToleranceDict = tolerances;
                    Updatable.WriteLoadedOffsets();
                }
            }
            else
            {
                Type = (T)Activator.CreateInstance(typeof(T));
                // Dynamically create type of undefined one. ... Use Loader with (dynamic/object??)   
                Updatable = new UpdatableType<T>(debug, processMethods, Type, InitOffsets(objName, props, out var tolerances), plugin, props);
                Updatable.ToleranceDict = tolerances;
                Updatable.WriteLoadedOffsets();
            }
        }

        private Dictionary<string, List<List<IntPtr>>> InitOffsets(out Dictionary<string, (int, int)> tolerances)
        {
            tolerances = new Dictionary<string, (int, int)>();
            var tuples = tolerances;
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
                    Debug.AddMessage<object>(new Message<object>("Offsets loading from file as defined type" +
                                                                 "\nPath of UpdatableType file: \n[" + Type.GetType().Name + "]=>[" + path + "]"));
                    foreach (var field in typeof(T).GetFields())
                    {
                        var adresses = new List<IntPtr>();
                        // offsetlist.InsertRange(0, reader.Element(field.Name)?.Attributes() .Cast<IntPtr>() ?? throw new InvalidOperationException());
                        if (!(reader.Root.Element(field.Name) is null) && reader.Root.Element(field.Name).HasAttributes)
                        {
                            offsetlists.Clear();
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
                                        if (offset.Length > 0)
                                        {
                                            val = new IntPtr(
                                                (uint)new System.ComponentModel.UInt32Converter()
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
                                    if (offsets.ContainsKey(field.Name))
                                    {
                                        offsets.TryGetValue(field.Name, out var list2);
                                        list2?.Add(new List<IntPtr>(adresses));
                                    }
                                    else
                                    {
                                        offsetlists.Add(new List<IntPtr>(adresses));
                                        offsets.Add(field.Name, new List<List<IntPtr>>(offsetlists));
                                    }
                                }
                                adresses.Clear();
                            }
                        }
                    }
                }
                return new List<object>() { offsets };
            });

            AddWork(task);

            task.Wait(-1);
            tolerances = tuples;
            return task.Result.First() as Dictionary<string, List<List<IntPtr>>>;
        }

        private Dictionary<string, List<List<IntPtr>>> InitOffsets(string objName, IEnumerable<string> props, out Dictionary<string, (int, int)> tolerances)
        {
            tolerances = new Dictionary<string, (int, int)>();
            Dictionary<string, (int, int)> tuples = tolerances;
            var task = new Task<List<object>>(() =>
            {
                var path = Directory.GetCurrentDirectory()
                           + "/Offsets/"
                           + _plugin.GetType().Name.Substring(0, _plugin.GetType().Name.Length)
                           + "/" + objName + ".xml";
                var xmlSerializer = new JsonFx.Xml.XmlWriter();
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
                        foreach (var field in props)
                        {
                            var adresses = new List<IntPtr>();
                            // offsetlist.InsertRange(0, reader.Element(field.Name)?.Attributes() .Cast<IntPtr>() ?? throw new InvalidOperationException());
                            if (!(reader.Root.Element(field) is null) && reader.Root.Element(field).HasAttributes)
                            {
                                offsetlists.Clear();
                                foreach (XAttribute list in reader.Root.Element(field)?.Attributes())
                                {
                                    if (list.Name.LocalName.ToLower().Contains("tolerance") && list.Value != " ")
                                    {
                                        if (tuples.ContainsKey(field)) tuples.Remove(field);
                                        tuples.Add(field,
                                            ((int) new System.ComponentModel.Int32Converter().ConvertFromString(list.Value.Split(' ').FirstOrDefault()),
                                                ((int) new System.ComponentModel.Int32Converter().ConvertFromString(
                                                    list.Value.Split(' ').LastOrDefault()))));
                                    }

                                    foreach (var offset in list.Value.Split(' '))
                                    {
                                        if (!list.Name.LocalName.ToLower().Contains("tolerance") && offset != " ")
                                        {
                                            if (offset.Length > 0)
                                            {
                                                val = new IntPtr(
                                                    (uint) new System.ComponentModel.UInt32Converter()
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
                    catch (Exception e)
                    {
                        Debug.AddMessage<object>(new Message<object>("Xml file is damaged. Remove it manually.\nException Message: {" + e.Message + "}", MessageTypeEnum.Exception));
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
