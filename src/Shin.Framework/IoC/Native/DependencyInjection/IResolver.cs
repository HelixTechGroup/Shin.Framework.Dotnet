#region Usings
#endregion

using System;

namespace Shin.Framework.IoC.Native.DependencyInjection
{
    internal interface IResolver : IDispose
    {
        #region Properties
        //Func<object[], object> CreateInstanceFunc { get; }
        bool Singleton { get;  }
        bool HasInstance { get; }
        Type Type { get; }
        #endregion

        #region Methods
        object CreateObject(params object[] parameters);
        object GetObject(params object[] parameters);
        #endregion
    }

    internal interface IResolver<T> : IResolver
    {
        #region Properties
        //new Func<object[], T> CreateInstanceFunc { get; set; }
        #endregion

        #region Methods
        new T CreateObject(params object[] parameters);
        new T GetObject(params object[] parameters);
        #endregion
    }
}