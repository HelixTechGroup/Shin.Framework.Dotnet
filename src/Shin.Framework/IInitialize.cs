#region Header
// solution:Shin.Framework.Dotnet
// project:	Shin.Framework
// file:	Shin.Framework\IInitialize.cs
// summary:	Declares the IInitialize interface
//			Copyright (c) 2023 HelixDesign, llc. All rights reserved.
#endregion

#region Usings
using System;
#endregion

namespace Shin.Framework
{
    /// <summary>Interface for initialize.</summary>
    public interface IInitialize : IDispose
    {
        #region Events
        event EventHandler Initialized; ///< Event queue for all listeners interested in Initialized events.
        event EventHandler Initializing;    ///< Event queue for all listeners interested in Initializing events.
        #endregion

        #region Properties

        /// <summary>Gets a value indicating whether this IInitialize is initialized.</summary>
        /// <value>True if this IInitialize is initialized, false if not.</value>
        bool IsInitialized { get; }
        #endregion

        #region Methods
        /// <summary>Initializes this IInitialize.</summary>
        void Initialize();
        #endregion
    }
}