using System;
using Microsoft.Extensions.Options;

namespace VX_ACE_IT_CORE.MVC.Model.Configuration.Options.Interfaces
{
    public interface IWritableOptions<out T> : IOptionsSnapshot<T> where T : class, new()
    {
        void Update(Action<T> applyChanges);
    }
}