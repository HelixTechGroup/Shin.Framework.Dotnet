namespace Shin.Framework.IoC.DependencyInjection
{
    /// <summary>Interface for IoC(Inversion of Control) bindings.</summary>
    public interface IBindings
    {
        #region Methods
        /// <summary>Loads this object.</summary>
        void Load();

        void Unload();
        #endregion
    }
}