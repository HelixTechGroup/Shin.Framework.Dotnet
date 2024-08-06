#region Usings
#endregion

#region Usings
using System;
#endregion

namespace Shin.Extensions
{
    public static class ExceptionExtensions
    {
        #region Methods
        public static Exception GetFrameworkException(this Exception exception)
        {
            return exception;
            //return IoCProvider.Container.Resolve<FrameworkExceptionProvider>()
            //        .GetFrameworkException(exception);
        }
        #endregion
    }
}