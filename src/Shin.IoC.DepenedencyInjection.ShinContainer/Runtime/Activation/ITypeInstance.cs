namespace Shin.IoC.DependencyInjection.Runtime.Activation
{
    internal interface ITypeInstance
    {
        ITypeResolver Resolver { get; }

        object Value { get; }

        void Inject(string propertyName, object value);

        void Inject(string methodName,
                                    params object[] arguments);
    }
}