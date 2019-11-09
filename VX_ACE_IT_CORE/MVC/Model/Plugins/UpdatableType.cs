using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Dynamic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using VX_ACE_IT_CORE.Debug;
using VX_ACE_IT_CORE.MVC.Model.Async;
using VX_ACE_IT_CORE.MVC.Model.GameProcess;
using VX_ACE_IT_CORE.MVC.Model.Plugins.GLOBAL_TYPES;
using VX_ACE_IT_CORE.MVC.Model.Plugins.RPGMAKER_VX_ACE.VX_ACE_TYPES;

namespace VX_ACE_IT_CORE.MVC.Model.Plugins
{
    public class UpdatableType<T> : BaseAsync<object>
    {
        public T Type;
        public Updatable<T> Updatable;
        /// <summary>
        /// This is not in type because: 
        /// <para> 1) You will have to declare it every time. </para>
        /// <para> 2) The class cannot be derived. (Would not recommend it.) </para>
        /// <para> 3) Class has to be super easy to declare and init. </para> 
        /// <para> (int,..) Bottom range (Not included) | (..,int) Upper range (Included) </para>
        /// </summary>
        public Dictionary<string, (int, int)> ToleranceDict = new Dictionary<string, (int, int)>();
        public readonly Dictionary<object, List<List<IntPtr>>> Offsets = new Dictionary<object, List<List<IntPtr>>>();
        private readonly ProcessMethods _processMethods;

        public UpdatableType(BaseDebug debug, ProcessMethods processMethods, T type, Dictionary<string, List<List<IntPtr>>> offsets, PluginBase module = null, IEnumerable<string> props = null)
            : base(debug, processMethods._gameProcess)
        {
            this.Type = type;
            this._processMethods = processMethods;
            Init(offsets, props);
            if (module != null) BeginUpdatePrimitives(module);

        }

        void Init(Dictionary<string, List<List<IntPtr>>> offsets, IEnumerable<string> props)
        {
            var tsk = new Task<List<object>>(() =>
            {
                //todo: Impl. non-defined types field detection.
                if (!(this.Type is ExpandoObject))
                    foreach (FieldInfo fieldInfo in Type.GetType().GetFields())
                    {
                        // Create plugin config, where will be desearialised class.
                        // idea -> could create entire from serializable document ? Let´s say player could be defined in txt.
                        // But that would require javascript dynamic access to object.... welp :-D
                        string fiName = offsets.Keys.FirstOrDefault((key => key == fieldInfo.Name));
                        if (fieldInfo.Name == fiName)
                        {
                            offsets.TryGetValue(fieldInfo.Name, out var intPtrs);
                            if (intPtrs != null)
                                Offsets.Add(fieldInfo, intPtrs);
                        }
                        // Something like this. Just create usable config file for this.
                        this.Updatable = new Updatable<T>(this.Type);
                    }
                else
                {
                    // Will use names of loaded offsets if none field names are specified.
                    // You will not see any null offsets.
                    if (!props.Any())
                    {
                        props = (offsets as IDictionary<string, List<List<IntPtr>>>)?.Keys;
                    }
                    this.Updatable = new Updatable<T>(props);
                    this.Type = Updatable.GetUpdatable;
                    var dictionary = (IDictionary<string, object>)(this.Type);
                    foreach (var dKey in dictionary.Keys)
                    {
                        // Create plugin config, where will be desearialised class.
                        // idea -> could create entire from serializable document ? Let´s say player could be defined in txt.
                        // But that would require javascript dynamic access to object.... welp :-D
                        string fiName = offsets.Keys.FirstOrDefault((key => key == dKey));
                        if (dKey == fiName)
                        {
                            offsets.TryGetValue(dKey, out var intPtrs);
                            if (intPtrs != null)
                                Offsets.Add(dKey, intPtrs); //todo: welp, ima fcked. Need better dict.
                        }
                    }
                }

                if (this.Type.GetType().GetFields().Length < Offsets.Count)
                {
                    // Create OnOffsetUpdate delegate.
                    if(!props.Any())
                    Debug.AddMessage<object>(new Message<object>(
                        "[" + GetType().Name + "][" + this.Type.GetType().Name + "] more offsets loaded than there are fields in the class. Check your config for additional lines.",
                        MessageTypeEnum.Indifferent));
                    else
                    if(props?.Count() < Offsets.Count)
                        Debug.AddMessage<object>(new Message<object>(
                            "[" + GetType().Name + "][" + this.Type.GetType().Name + "(UserDefined)] more offsets loaded than there are fields in the class. Check your config for additional lines.",
                            MessageTypeEnum.Indifferent));
                }
                else
                if (this.Type.GetType().GetFields().Count() == offsets.Count())
                {
                    // Create OnOffsetUpdate delegate.
                    Debug.AddMessage<object>(new Message<object>(
                        "[" + GetType().Name + "][" + this.Type.GetType().Name + "] all offsets loaded succesfully. (Count): [" + this.Type.GetType().GetFields().Count() + "]",
                        MessageTypeEnum.Event));
                }
                else if (this.Type.GetType().GetFields().Count() > offsets.Count() && offsets.Any())
                {
                    Debug.AddMessage<object>(new Message<object>(
                        "[" + GetType().Name + "][" + this.Type.GetType().Name + "] only some of the offsets were loaded.",
                        MessageTypeEnum.Indifferent));
                    //Offsets.TryGetValue(typeof(Player).GetField("Hp"), out var list);
                }
                else if (!offsets.Any())
                {
                    Debug.AddMessage<object>(new Message<object>(
                        "[" + GetType().Name + "][" + this.Type.GetType().Name + "] none offsets were loaded.",
                        MessageTypeEnum.Error));
                }// else if (offsets.Count > 0) WriteLoadedOffsets();
                if (offsets.Any() && ((Type as ExpandoObject) != null))
                {
                    //Is called in offsetLoader, because of tolerances.
                    Debug.AddMessage<object>(new Message<object>(
                        "[User Defined][" + this.Type.GetType().Name + "] type was loaded. (No way to check if type is valid/useful)",
                        MessageTypeEnum.Indifferent));
                }
                return null;
            });
            AddWork(tsk);
            tsk.Wait(-1);
        }

        public void BeginUpdatePrimitives(PluginBase pluginBase)
        {
            AddWork(new Task<List<object>>(() =>
            {
                if (pluginBase.ModuleBaseAddr == IntPtr.Zero) Debug.AddMessage<object>(new Message<object>("Module address is not set. Engine values cannot be read.", MessageTypeEnum.Error));
                var occurences = new List<KeyValuePair<IntPtr, int>>();
                while (true)
                {
                    int readAddressVal = 0; // Not used I know, but can be moved to field ? or even as Type Field pair
                    foreach (var keyPar in Offsets)
                    {
                        string keyName = (keyPar.Key as FieldInfo) == null ? keyPar.Key.ToString() : ((FieldInfo) keyPar.Key).Name;
                        if (ToleranceDict.TryGetValue(keyName, out var toleranceTuple))
                            foreach (var offSetList in keyPar.Value)
                            {
                                readAddressVal = _processMethods.Rpm<int>(pluginBase.ModuleBaseAddr, offSetList, out var valAdress);
                                if (readAddressVal > toleranceTuple.Item1 && readAddressVal <= toleranceTuple.Item2)
                                {

                                    occurences.Add(new KeyValuePair<IntPtr, int>(valAdress, readAddressVal));
                                    // Debug.AddMessage<object>(new Message<object>("HP:"+valAdress.ToString("X")));
                                    // KeyPar.Key.SetValue(Type, i);
                                }
                            }
                        else
                            foreach (var offSetList in keyPar.Value)
                            {
                                readAddressVal = _processMethods.Rpm<int>(pluginBase.ModuleBaseAddr, offSetList, out var valAdress);
                                if (valAdress != IntPtr.Zero)
                                {
                                    occurences.Add(new KeyValuePair<IntPtr, int>(valAdress, readAddressVal));
                                    // Debug.AddMessage<object>(new Message<object>("HP:"+valAdress.ToString("X")));
                                    // KeyPar.Key.SetValue(Type, i);
                                }
                            }
                        if (occurences.Any())
                        {
                            // Theory is, that by the count of multipointer read values, most occured one will be the searched one.
                            var grouped = occurences.ToLookup(x => x);
                            
                            if (grouped.Any())
                            {
                                //Get Type, get fields by keypars, set them and debug.
                                var dict = Type as IDictionary<string, object>;
                                if (dict == null)
                                {
                                    Type.GetType().GetField(keyName).SetValue(Type,
                                        new KeyValuePair<IntPtr, Numeric<int>>(
                                            grouped.FirstOrDefault().Key.Key,
                                            new Numeric<int>(grouped.FirstOrDefault().Key.Value)));
                                    Thread.Sleep(Precision);
                                    Debug.AddMessage<object>(new Message<object>(Type.ToString()));
                                }
                                else
                                {
                                    lock (dict)
                                    {
                                        dict[keyName] = new KeyValuePair<IntPtr, Numeric<int>>(
                                            grouped.FirstOrDefault().Key.Key,
                                            new Numeric<int>(grouped.FirstOrDefault().Key.Value));
                                        Thread.Sleep(Precision);
                                        Debug.AddMessage<object>(new Message<object>(Updatable.ToString()));
                                    }        
                                }
                            }
                        }
                        occurences.Clear();
                    }
                    Thread.Sleep(Precision);
                }
            }));
        }

        public void SetValue<TP>(string fieldName, TP t) where TP : struct
        {
            _processMethods.Wpm(((dynamic)Type.GetType().GetField(fieldName).GetValue(Type)).Key, t);
        }

        public string ToDebugString(IDictionary<object, List<List<IntPtr>>> dictionary)
        {
            //todo: fix this and replace.
            //var key = dictionary.Keys.FirstOrDefault() as FieldInfo;
            //return "{" + string.Join(",", dictionary.Select(kv => "\n" +
            //                                                      (key == null ? kv.Key.ToString() : ((FieldInfo)kv.Key).Name)) + "=" +
            //                                                      WriteInpPtrList(kv.Value, ((kv.Key as FieldInfo) == null ? kv.Key.ToString() : ((FieldInfo)kv.Key).Name)).ToArray()) + "\n}<" +
            //       Type.GetType().Name + ">");
            if (dictionary.Keys.FirstOrDefault() is FieldInfo)
                return "{" + string.Join(",", dictionary.Select(kv => "\n" +
                                                                      ((FieldInfo)kv.Key).Name + "=" +
                                                                      WriteInpPtrList(kv.Value, ((FieldInfo)kv.Key).Name)).ToArray()) + "\n}<" +
                       Type.GetType().Name + ">";
            return "{" + string.Join(",", dictionary.Select(kv => "\n" +
                                                                  kv.Key + "=" +
                                                                  WriteInpPtrList(kv.Value, kv.Key.ToString())).ToArray()) + "\n}<" +
                   Type.GetType().Name + ">";
        }

        public void WriteLoadedOffsets()
        {
            var dict = string.Join(";", Offsets);
            Debug.AddMessage<object>(new Message<object>(
                "\n--[Init][" + this.Type.GetType().Name + "]--" + ToDebugString(Offsets) + "\n------------------"
                , MessageTypeEnum.Standard));
        }

        private string WriteInpPtrList(List<List<IntPtr>> lists, string name)
        {
            //I know this is not nice solution, but what gives. No performance benefit 
            //from optimalisation, since this is just a init.
            int offsetId = 0;
            string strList = "{";
            foreach (var list in lists)
            {
                strList += "\n[" + offsetId++ + "]{";
                foreach (IntPtr val in list)
                {
                    strList += "[0x" + val.ToString("X") + "]";
                }
                if (list.Count == 0) strList += "empty";
                strList += "}";
            }

            if (ToleranceDict.TryGetValue(name, out var value))
                strList += "\nValTolerance:[" + value + "]}";
            else
                strList += "\nValTolerance:[Not yet loaded/null]}";
            return strList;
        }
    }
}
