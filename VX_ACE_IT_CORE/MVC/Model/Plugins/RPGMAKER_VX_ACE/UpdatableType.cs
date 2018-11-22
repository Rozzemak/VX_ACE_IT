using System;
using System.Collections.Generic;
using System.ComponentModel;
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
using VX_ACE_IT_CORE.MVC.Model.Plugins.RPGMAKER_VX_ACE.VX_ACE_TYPES;

namespace VX_ACE_IT_CORE.MVC.Model.Plugins.RPGMAKER_VX_ACE
{
    public class UpdatableType<T> : BaseAsync<object>
    {
        public T Type;
        /// <summary>
        /// This is not in type because: 
        /// <para> 1) You will have to declare it every time. </para>
        /// <para> 2) The class cannot be derived. (Would not recommend it.) </para>
        /// <para> 3) Class has to be super easy to declare and init. </para> 
        /// <para> (int,..) Bottom range (Not included) | (..,int) Upper range (Included) </para>
        /// </summary>
        public Dictionary<string, (int, int)> ToleranceDict = new Dictionary<string, (int, int)>();
        public readonly Dictionary<FieldInfo, List<List<IntPtr>>> Offsets = new Dictionary<FieldInfo, List<List<IntPtr>>>();
        private readonly ProcessMethods _processMethods;

        public UpdatableType(BaseDebug debug, ProcessMethods processMethods, T type, Dictionary<string, List<List<IntPtr>>> offsets, PluginBase module = null)
            : base(debug, processMethods._gameProcess)
        {
            this.Type = type;
            this._processMethods = processMethods;
            Init(offsets);
            if (module != null) BeginUpdatePrimitives(module);
        }

        void Init(Dictionary<string, List<List<IntPtr>>> offsets)
        {
            var tsk = new Task<List<object>>(() =>
            {
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
                }

                if (this.Type.GetType().GetFields().Length < Offsets.Count)
                {
                    // Create OnOffsetUpdate delegate.
                    Debug.AddMessage<object>(new Message<object>(
                        "[" + GetType().Name + "][" + this.Type.GetType().Name + "] more offsets loaded than there are fields in the class. Check your config for additional lines.",
                        MessageTypeEnum.Indifferent));
                }
                else
                if (this.Type.GetType().GetFields().Count() == offsets.Count())
                {
                    // Create OnOffsetUpdate delegate.
                    Debug.AddMessage<object>(new Message<object>(
                        "[" + GetType().Name + "][" + this.Type.GetType().Name + "] all offsets loaded succesfully. (Count): ["+ this.Type.GetType().GetFields().Count()+"]",
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
                }
                else { } // Wtf hapened here ?!
                if (offsets.Count > 0)
                {
                    var dict = string.Join(";", Offsets);
                    Debug.AddMessage<object>(new Message<object>(
                        "[" + this.Type.GetType().Name + "]" + ToDebugString(Offsets)
                        , MessageTypeEnum.Standard));
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
                var occurences = new List<KeyValuePair<IntPtr, int>>();
                while (true)
                {
                    if (pluginBase.ModuleBaseAddr == IntPtr.Zero) Debug.AddMessage<object>(new Message<object>("Module address is not set. Engine values cannot be read.", MessageTypeEnum.Error));
                    int readAddressVal = 0; // Not used I know, but can be moved to field ? or even as Type Field pair
                    foreach (var keyPar in Offsets)
                    {
                        ToleranceDict.TryGetValue(keyPar.Key.Name, out var toleranceTuple);
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
                        if (occurences.Any())
                        {
                            // Theory is, that by the count of multipointer read values, most occured one will be the searched one.
                            var grouped = occurences.ToLookup(x => x);

                            if (grouped.Any())
                            {
                                //Get Type, get fields by keypars, set them and debug.
                                Type.GetType().GetField(keyPar.Key.Name).SetValue(Type,
                                new KeyValuePair<IntPtr, Numeric<int>>(
                                    grouped.FirstOrDefault().Key.Key,
                                    new Numeric<int>(grouped.FirstOrDefault().Key.Value)));
                                Thread.Sleep(Precision);
                                Debug.AddMessage<object>(new Message<object>(Type.ToString()));
                            }
                        }
                        occurences.Clear();
                    }
                    Thread.Sleep(Precision);
                }
            }));
        }

        public string ToDebugString(IDictionary<FieldInfo, List<List<IntPtr>>> dictionary)
        {
            return "{" + string.Join(",", dictionary.Select(kv => kv.Key.Name + "=" + WriteInpPtrList(kv.Value)).ToArray()) + "}";
        }

        private string WriteInpPtrList(List<List<IntPtr>> lists)
        {
            //I know this is not nice solution, but what gives. No performance benefit 
            //from optimalisation, since this is just a init.
            string strList = "";
            foreach (var list in lists)
            {
                strList += "{";
                foreach (IntPtr val in list)
                {
                    strList += "[" + val.ToString("X") + "]";
                }
                if (list.Count == 0) strList += "empty";
                strList += "}";
            }
            return strList;
        }
    }
}
