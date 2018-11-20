using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
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
        private PluginBase _plugin;

        public OffsetLoader(BaseDebug debug, ProcessMethods processMethods, PluginBase plugin, int precision = 33, bool defined = true)
        : base(debug, processMethods._gameProcess, precision)
        {
            this._plugin = plugin;
            if (defined)
            {
                Type = (T)Activator.CreateInstance(typeof(T));
                if (!(Type.Equals(default(object)))) // At least something defined was created.
                {
                    Updatable = new UpdatableType<T>(debug, processMethods, Type, InitOffsets(Type));
                    InitOffsets(Type);
                }
            }
            else
            {
                // Dynamically create type of undefined one. ... Use Loader with (dynamic/object??)   
            }

        }

        Dictionary<string, List<List<IntPtr>>> InitOffsets(T type)
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
                                          + _plugin.GetType().Name.Substring(0, _plugin.GetType().Name.Length) + "/");
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
                            foreach (var offset in list.Value.Split(' '))
                            {
                                if (offset != " ")
                                {
                                    val = new IntPtr(
                                        (uint) new System.ComponentModel.UInt32Converter().ConvertFromString(offset));
                                }
                                if(val != IntPtr.Zero)
                                adresses.Add(val);
                                val = IntPtr.Zero;
                            }                        
                        }
                    }
                    
                    offsetlists.Add(adresses);
                }   
                foreach (var field in typeof(T).GetFields())
                {
                    offsets.Add(field.Name, new List<List<IntPtr>>(offsetlists));
                }
                offsetlists.Clear();
            }

            //val = offsets.First();

            return offsets;
        }


    }
}
