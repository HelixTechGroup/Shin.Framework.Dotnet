#region Usings
#endregion

#region Usings
using System.Collections.Generic;
using Shin.Framework.Exceptions;
#endregion

namespace Shin.Framework
{
    public class ThrowExceptionSelector
    {
        #region Members
        private readonly bool m_predicate;
        #endregion

        public ThrowExceptionSelector(bool predicate)
        {
            m_predicate = predicate;
        }

        #region Methods
        public void ArgumentNullException(string paramName,
                                          string message = null,
                                          params KeyValuePair<string, object>[] data)
        {
            if (m_predicate)
                throw ExceptionProvider.ArgumentNullException(paramName, message, data);
        }

        public void ArgumentOutOfRangeException(string paramName,
                                                object actualValue = null,
                                                string message = null,
                                                params KeyValuePair<string, object>[] data)
        {
            if (m_predicate)
                throw ExceptionProvider.ArgumentOutOfRangeException(paramName, actualValue, message, data);
        }

        public void ArgumentException(string paramName,
                                      string message = null,
                                      params KeyValuePair<string, object>[] data)
        {
            if (m_predicate)
                throw ExceptionProvider.ArgumentException(paramName, message, data);
        }

        public void InvalidOperationException(string message = null,
                                              params KeyValuePair<string, object>[] data)
        {
            if (m_predicate)
                throw ExceptionProvider.InvalidOperationException(message, data);
        }

        public void UnauthorizedAccessException(string message = null,
                                                params KeyValuePair<string, object>[] data)
        {
            if (m_predicate)
                throw ExceptionProvider.UnauthorizedAccessException(message, data);
        }

        public void IOException(string message = null,
                                params KeyValuePair<string, object>[] data)
        {
            if (m_predicate)
                throw ExceptionProvider.IOException(message, data);
        }
        #endregion
    }
}