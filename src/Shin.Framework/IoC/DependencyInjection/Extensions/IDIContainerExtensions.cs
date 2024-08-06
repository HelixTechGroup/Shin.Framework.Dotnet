using System;

namespace Shin.IoC.DependencyInjection.Extensions
{
    public static class IDIContainerExtensions
    {
        public static IDIContainer CreateChildContainer(this IDIContainer container)
        {
            return ((IDIParentContainer)container).CreateChildContainer();
        }

        public static T Resolve<T>(this IDIContainer container, params object[] parameters)
        {
            return container.Resolve<T>(null, parameters: parameters);
        }

        public static object Resolve(this IDIContainer container,
                                     Type type, 
                                   params object[] parameters)
        {
            return container.Resolve(type, parameters: parameters);
        }
        
        //public static bool IsTypeRegisteredParentOnly(this DIContainer container)
        //{
        //    container.RegisteredTypes
        //}
    }
}
