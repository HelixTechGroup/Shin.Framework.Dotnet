#region Usings
#endregion

#region Usings
using System.Linq;
#endregion

namespace Shin.Framework.Extensions
{
    public static class StringExtensions
    {
        #region Methods
        public static string Inject(this string format, params object[] formattingArgs)
        {
            return string.Format(format, formattingArgs);
        }

        public static string Inject(this string format, params string[] formattingArgs)
        {
            return string.Format(format, formattingArgs.Select(a => a as object).ToArray());
        }
        #endregion
    }
}