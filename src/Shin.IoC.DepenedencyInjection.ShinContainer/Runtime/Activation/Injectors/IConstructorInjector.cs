namespace Shin.IoC.DependencyInjection.Runtime.Activation.Injectors
{
    public interface IConstructorInjector : IInjector
    {
        object Inject(params object[] parameters);
    }
}