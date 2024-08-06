#region Usings
using System.Collections.Generic;
using System.Diagnostics;
using Shin.Exceptions;
#endregion

namespace Shin
{
    public class ThrowExceptionBuilder : IFluentInterface
    {
        #region Members
        private readonly bool m_predicate;
        #endregion

        public ThrowExceptionBuilder(bool predicate) { m_predicate = predicate; }

        #region Methods
        public void ArgumentNullException(string paramName,
                                          string message = null,
                                          StackFrame frame = default,
                                          params KeyValuePair<string, object>[] data)
        {
            if (m_predicate) throw ExceptionProvider.ArgumentNullException(paramName, message, frame, data);
        }

        public void ArgumentOutOfRangeException(string paramName,
                                                object actualValue = null,
                                                string message = null,
                                                StackFrame frame = default,
                                                params KeyValuePair<string, object>[] data)
        {
            if (m_predicate) throw ExceptionProvider.ArgumentOutOfRangeException(paramName, actualValue, message, frame, data);
        }

        public void ArgumentException(string paramName,
                                      string message = null,
                                      StackFrame frame = default,
                                      params KeyValuePair<string, object>[] data)
        {
            if (m_predicate) throw ExceptionProvider.ArgumentException(paramName, message, frame, data);
        }

        public void InvalidOperationException(string message = null,
                                              StackFrame frame = default,
                                              params KeyValuePair<string, object>[] data)
        {
            if (m_predicate) throw ExceptionProvider.InvalidOperationException(message, frame, data);
        }

        public void UnauthorizedAccessException(string message = null,
                                                StackFrame frame = default,
                                                params KeyValuePair<string, object>[] data)
        {
            if (m_predicate) throw ExceptionProvider.UnauthorizedAccessException(message, frame, data);
        }

        public void IOException(string message = null,
                                StackFrame frame = default,
                                params KeyValuePair<string, object>[] data)
        {
            if (m_predicate) throw ExceptionProvider.IOException(message, frame, data);
        }
        #endregion
    }
}