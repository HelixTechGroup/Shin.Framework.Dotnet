namespace Shin.Framework.Logging
{
    /// <summary>
    /// Defines values for the priorities used by <see cref="ILogProvider"/>.
    /// </summary>
    public enum LogPriority
    {
        /// <summary>
        /// No priority specified.
        /// </summary>
        None = 0,

        /// <summary>
        /// High priority entry.
        /// </summary>
        High = 1,

        /// <summary>
        /// Medium priority entry.
        /// </summary>
        Medium,

        /// <summary>
        /// Low priority entry.
        /// </summary>
        Low
    }
}