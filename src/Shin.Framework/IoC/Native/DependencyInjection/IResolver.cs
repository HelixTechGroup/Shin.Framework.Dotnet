#region Usings
using System;
using Shin.Framework;
#endregion

namespace Shield.Framework.IoC.Native.DependencyInjection
{
    internal interface IResolver : IDispose
    {
        #region Properties
        Func<object[], object> CreateInstanceFunc { get; set; }
        bool Singleton { get; set; }
        bool HasInstance { get; }
        Type Type { get; set; }
        #endregion

        #region Methods
        object GetObject(params object[] parameters);
        #endregion
    }

    internal interface IResolver<T> : IResolver
    {
        #region Properties
        new Func<object[], T> CreateInstanceFunc { get; set; }
        #endregion

        #region Methods
        new T GetObject(params object[] parameters);
        #endregion
    }
}