using System;

namespace Shin.IoC.DependencyInjection
{
    public interface IDIRegistrationBuilder<T> : IFluentInterface
    {
        IDIRegistrationToType<T> ToType<T>();

        IDIRegistrationToType<T> ToType(Type T);

        IDIRegistrationToValue<T> ToValue<T>(T value);

        IDIRegistrationToValue<T> ToValue(object value);
    }
    
    public interface IDIRegistrationToType<T> : IDIRegistrationTo<T>
    {
        
    }

    public interface IDIRegistrationToValue<T> : IDIRegistrationTo<T> { }

    public interface IDIRegistrationWithLifetime : IFluentInterface
    {
        
    }

    public interface IDIRegistrationTo<T>
    {
        IDIRegistrationWithLifetime WithLifetime();
    }
}