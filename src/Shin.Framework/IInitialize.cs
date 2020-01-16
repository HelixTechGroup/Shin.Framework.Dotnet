﻿#region Usings
#endregion

#region Usings
using System;
#endregion

namespace Shin.Framework
{
    public interface IInitialize
    {
        #region Events
        event EventHandler Initializing;
        event EventHandler Initialized;
        #endregion

        #region Properties
        bool IsInitialized { get; }
        #endregion

        #region Methods
        void Initialize();
        #endregion
    }
}