#region Usings
using System;
#endregion

namespace Shin.Framework.Messaging
{
    /// <summary>   Interface for delegate reference. </summary>
    internal interface IDelegateReference
    {
        #region Properties
        Delegate Target { get; }
        #endregion
    }
}