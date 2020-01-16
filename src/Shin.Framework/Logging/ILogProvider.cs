#region Usings
#endregion

namespace Shin.Framework.Logging
{
    public interface ILogProvider : IDispose
    {
        #region Methods
        void Flush(ILogEntry entry);
        #endregion
    }
}