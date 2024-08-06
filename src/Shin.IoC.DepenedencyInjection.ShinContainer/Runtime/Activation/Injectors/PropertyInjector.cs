#region Usings
using System;
using System.Reflection;
#endregion

namespace Shin.IoC.DependencyInjection.Runtime.Activation.Injectors
{
    public struct PropertyInjector : IPropertyInjector
    {
        #region Members
        internal readonly PropertyInfo Property;
        private readonly Delegate m_propertySetDelegate;
        #endregion

        #region Properties
        public string Name
        {
            get { return Property?.Name; }
        }

        /// <inheritdoc />
        public Type Type
        {
            get { return Property?.PropertyType; }
        }
        #endregion

        public PropertyInjector(PropertyInfo property)
        {
            Property = property;
            m_propertySetDelegate = ExpressionCompiler.CreateDelegate(property);
        }

        #region Methods
        public object Inject(ref object instance,
                             object value)
        {
            return m_propertySetDelegate?.DynamicInvoke(instance, value);
        }
        #endregion
    }
}