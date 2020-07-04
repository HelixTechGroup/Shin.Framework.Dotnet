#region Usings
#endregion

namespace Shin.Framework.Logging
{
    public interface ILogProvider : IDispose
    {
        #region Properties
        bool IsBuffering { get; }
        #endregion

        #region Methods
        void Flush(ILogEntry entry);
        #endregion
    }
}