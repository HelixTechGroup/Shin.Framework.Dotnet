#region Usings
#endregion

using System;
using System.Collections.Generic;

using Shin.IoC.DependencyInjection.Runtime.Activation.Injectors;

namespace Shin.IoC.DependencyInjection.Runtime.Activation
{
    internal partial interface ITypeResolver : IId<Guid>, IInitialize, IDispose
    {
        #region Properties
        //Func<object[], object> CreateInstanceFunc { get; }
        //bool Singleton { get;  }
        //bool HasInstance { get; }
        Type Type { get; }

        //IReadOnlyCollection<Guid> Interfaces { get; }
        //IReadOnlyCollection<IDependencyGraph> Dependencies { get; }
        //IReadOnlyCollection<IConstructorInjector> Constructors { get; }
        //IReadOnlyCollection<IPropertyInjector> Properties { get; }
        //IReadOnlyCollection<IMethodInjector> Methods { get; }
        #endregion

        #region Methods
        //bool CheckInterface(Type interfaceType);
        //bool CheckInterface<T>();
        //bool CheckDependencies(Type dependencyType);
        //bool CheckDependencies<T>();
        ITypeInstance Construct(params object[] arguments);
        //ITypeResolverBuilder Build();
        #endregion
    }

    //internal interface ITypeResolver<out T> : ITypeResolver
    //{
    //    #region Properties
    //    //new Func<object[], T> CreateInstanceFunc { get; set; }
    //    #endregion

    //    #region Methods
    //    //T CreateObject(params object[] parameters);
    //    new T GetInstance(params object[] parameters);
    //    #endregion
    //}
}