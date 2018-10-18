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
        public Dictionary<FieldInfo, List<List<IntPtr>>> Offsets = new Dictionary<FieldInfo, List<List<IntPtr>>>();
        private ProcessMethods _processMethods;

        public UpdatableType(BaseDebug debug, ProcessMethods processMethods, T type, Dictionary<string, List<List<IntPtr>>> offsets, VxAceModule vxAceModule = null)
            : base(debug, processMethods._gameProcess)
        {
            this.Type = type;
            this._processMethods = processMethods;
            Init(offsets);
            if (vxAceModule!= null) UpdatePrimitives(vxAceModule);
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
                if (this.Type.GetType().GetFields().Length == Offsets.Count)
                {
                    // Create OnOffsetUpdate delegate.
                    Debug.AddMessage<object>(new Message<object>(
                        "[" + GetType().Name + "][" + this.Type.GetType().Name + "] all offsets loaded succesfully.",
                        MessageTypeEnum.Standard));
                }
                else if (this.Type.GetType().GetFields().Length > 0)
                {
                    Debug.AddMessage<object>(new Message<object>(
                        "[" + GetType().Name + "][" + this.Type.GetType().Name + "] only some of the offsets were loaded.",
                        MessageTypeEnum.Indifferent));
                    //Offsets.TryGetValue(typeof(Player).GetField("Hp"), out var list);
                }
                else if (this.Type.GetType().GetFields().Length == 0)
                {
                    Debug.AddMessage<object>(new Message<object>(
                        "[" + GetType().Name + "][" + this.Type.GetType().Name + "] none offsets were loaded.",
                        MessageTypeEnum.Error));
                }
                else { } // Wtf hapened here ?!
                if (this.Type.GetType().GetFields().Length > 0)
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

        void UpdatePrimitives(VxAceModule vxAceModule)
        {
            AddWork(new Task<List<object>>(() =>
            {
                Thread.Sleep(3000);
                while (true)
                {
                    int i;
                    foreach (var KeyPar in Offsets)
                    {
                        try
                        {
                            foreach (var offSetList in KeyPar.Value)
                            {
                                i = _processMethods.Rpm<int>(vxAceModule.RgssBase, offSetList);
                                if (i > 0 && i < 10000)
                                {
                                   // KeyPar.Key.SetValue(Type, i);

                                   Type.GetType().GetField(KeyPar.Key.Name).SetValue(Type, new Numeric<int>(i));
                                }
                            }
                            Thread.Sleep(Precision);
                            Debug.AddMessage<object>(new Message<object>(Type.ToString()));
                        }
                        catch (Exception e)
                        {
                        }
                    }
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
