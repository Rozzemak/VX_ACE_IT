using System;

namespace VX_ACE_IT_CORE.MVC.Model.Plugins.Interfaces
{
    public interface IPluginBase
    {
        void Init(string moduleName, Action initUpdatablesAction);

        public void AddUpdatable<T>(UpdatableType<T> updatableType, bool update = true);

        public void UpdateBaseAddress();
    }
}