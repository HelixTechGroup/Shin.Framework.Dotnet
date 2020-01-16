#region Usings
#endregion

#region Usings
using System;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Threading;
#endregion

namespace Shin.Framework.Extensions
{
    public static class EventHandlerExtensions
    {
        #region Methods
        public static void Raise<TEventArgs>(this EventHandler<TEventArgs> handler, object sender, TEventArgs args)
            //where TEventArgs : EventArgs
        {
            Interlocked.CompareExchange(ref handler, null, null)?.Invoke(sender, args);
        }

        public static void Raise(this EventHandler handler, object sender, EventArgs args)
        {
            Interlocked.CompareExchange(ref handler, null, null)?.Invoke(sender, args);
        }

        public static void Raise(this PropertyChangedEventHandler handler, object sender, PropertyChangedEventArgs args)
        {
            Interlocked.CompareExchange(ref handler, null, null)?.Invoke(sender, args);
        }

        public static void Raise(this NotifyCollectionChangedEventHandler handler, object sender, NotifyCollectionChangedEventArgs args)
        {
            Interlocked.CompareExchange(ref handler, null, null)?.Invoke(sender, args);
        }

        public static void Dispose<TEventArgs>(this EventHandler<TEventArgs> handler)
        {
            if (handler != null)
            {
                foreach (var d in handler.GetInvocationList())
                    handler -= d as EventHandler<TEventArgs>;
            }
        }

        public static void Dispose(this EventHandler handler)
        {
            if (handler != null)
            {
                foreach (var d in handler.GetInvocationList())
                    handler -= d as EventHandler;
            }
        }
        #endregion
    }
}