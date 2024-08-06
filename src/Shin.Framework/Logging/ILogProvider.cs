#region Usings
#endregion

namespace Shin.Logging
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