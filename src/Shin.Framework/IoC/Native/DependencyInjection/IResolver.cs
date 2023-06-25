#region Usings
#endregion

using System;

namespace Shin.Framework.IoC.Native.DependencyInjection
{
    internal interface IResolver : IInitialize, IDispose
    {
        #region Properties
        //Func<object[], object> CreateInstanceFunc { get; }
        bool Singleton { get;  }
        bool HasInstance { get; }
        Type Type { get; }
        Type[] Interfaces { get; }
        #endregion

        #region Methods
        bool CheckInterface(Type interfaceType);
        bool CheckInterface<T>();
        //object CreateObject(params object[] parameters);
        object GetObject(params object[] parameters);
        #endregion
    }

    internal interface IResolver<out T> : IResolver
    {
        #region Properties
        //new Func<object[], T> CreateInstanceFunc { get; set; }
        #endregion

        #region Methods
        //T CreateObject(params object[] parameters);
        new T GetObject(params object[] parameters);
        #endregion
    }
}