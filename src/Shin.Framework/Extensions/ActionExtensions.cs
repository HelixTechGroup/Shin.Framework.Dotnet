#region Usings
#endregion

namespace Shin.Framework.Extensions
{
    public static class ActionExtensions
    {
        //#region UIThread
        //public static void OnUiThread(this Action action, Action callback = null)
        //{
        //    Shield.Instance.Dispatcher.UiDispatcher.Run(action, callback);
        //}

        //public static Task OnUiThreadAsync(this Action action, Action<Task> callback = null)
        //{
        //    return Shield.Instance.Dispatcher.UiDispatcher.RunAsync(action, callback);
        //}

        //public static void OnUiThread<T>(this Action<T> action, T parameter, Action callback = null)
        //{
        //    Shield.Instance.Dispatcher.UiDispatcher.Run(action, parameter, callback);
        //}

        //public static Task OnUiThreadAsync<T>(this Action<T> action, T parameter, Action<Task> callback = null)
        //{
        //    return Shield.Instance.Dispatcher.UiDispatcher.RunAsync(action, parameter, callback);
        //}
        //#endregion

        //#region BackgroundThread
        //public static void OnNewThread(this Action action, Action callback = null)
        //{
        //    Shield.Instance.Dispatcher.BackgroundDispatcher.Run(action, callback);
        //}

        //public static Task OnNewThreadAsync(this Action action, Action<Task> callback = null)
        //{
        //    return Shield.Instance.Dispatcher.BackgroundDispatcher.RunAsync(action, callback);
        //}

        //public static void OnNewThread<T>(this Action<T> action, T parameter, Action callback = null)
        //{
        //    Shield.Instance.Dispatcher.BackgroundDispatcher.Run(action, parameter, callback);
        //}

        //public static Task OnNewThreadAsync<T>(this Action<T> action, T parameter, Action<Task> callback = null)
        //{
        //    return Shield.Instance.Dispatcher.BackgroundDispatcher.RunAsync(action, parameter, callback);
        //}
        //#endregion

        //#region ContextThread
        //public static void OnNewThread(this Action action, SynchronizationContext context, Action callback = null)
        //{
        //    Shield.Instance.Dispatcher.ContextDispatcher.SetContext(context).Run(action, callback);
        //}

        //public static Task OnNewThreadAsync(this Action action, SynchronizationContext context, Action<Task> callback = null)
        //{
        //    return Shield.Instance.Dispatcher.ContextDispatcher.SetContext(context).RunAsync(action, callback);
        //}

        //public static void OnNewThread<T>(this Action<T> action, SynchronizationContext context, T parameter, Action callback = null)
        //{
        //    Shield.Instance.Dispatcher.ContextDispatcher.SetContext(context).Run(action, parameter, callback);
        //}

        //public static Task OnNewThreadAsync<T>(this Action<T> action, SynchronizationContext context, T parameter, Action<Task> callback = null)
        //{
        //    return Shield.Instance.Dispatcher.ContextDispatcher.SetContext(context).RunAsync(action, parameter, callback);
        //}
        //#endregion
    }
}