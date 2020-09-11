namespace Shin.Framework.Logging
{
    /// <summary>
    /// Defines values for the categories used by <see cref="ILogProvider"/>.
    /// </summary>
    public enum LogLevel
    {
        /// <summary>
        /// Debug category.
        /// </summary>
        Debug,

        /// <summary>
        /// Exception category.
        /// </summary>
        Exception,

        /// <summary>
        /// Informational category.
        /// </summary>
        Info,

        /// <summary>
        /// Warning category.
        /// </summary>
        Warn,
        None
    }
}