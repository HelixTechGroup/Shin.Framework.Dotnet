#region Usings
using System;
using Shin.Framework;
#endregion

namespace Shield.Framework.IoC.Native.DependencyInjection
{
    internal interface IResolver : IDispose
    {
        #region Properties
        Func<object> CreateInstanceFunc { get; set; }
        bool Singleton { get; set; }
        #endregion

        #region Methods
        object GetObject();
        #endregion
    }

    internal interface IResolver<T> : IResolver
    {
        #region Properties
        new Func<T> CreateInstanceFunc { get; set; }
        #endregion

        #region Methods
        new T GetObject();
        #endregion
    }
}