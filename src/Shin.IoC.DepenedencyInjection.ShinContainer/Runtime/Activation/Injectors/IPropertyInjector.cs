namespace Shin.IoC.DependencyInjection.Runtime.Activation.Injectors
{
    public interface IPropertyInjector : IInjector
    {
        bool HasDefaultValue { get; }

        object Inject(ref object instance,
                      object value);
    }
}