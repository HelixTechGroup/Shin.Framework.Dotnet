#region Usings
using System;
#endregion

namespace Shin
{
    /// <summary>   Interface for delegate reference. </summary>
    internal interface IDelegateReference
    {
        #region Properties
        Delegate Target { get; }
        #endregion
    }
}