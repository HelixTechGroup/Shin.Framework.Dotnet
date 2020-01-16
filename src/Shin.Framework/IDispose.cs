#region Usings
#endregion

#region Usings
using System;
#endregion

namespace Shin.Framework
{
    /// <inheritdoc />
    /// <summary>Interface for dispose.</summary>
    public interface IDispose : IDisposable
    {
        #region Events
        event EventHandler Disposing;
        event EventHandler Disposed;
        #endregion

        #region Properties
        ///-------------------------------------------------------------------------------------------------
        /// <summary>Gets a value indicating whether the disposed.</summary>
        ///
        /// <value>true if disposed, false if not.</value>
        ///-------------------------------------------------------------------------------------------------
        bool IsDisposed { get; }
        #endregion
    }
}