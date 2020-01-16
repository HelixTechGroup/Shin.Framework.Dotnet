#region Usings
#endregion

#region Usings
using System;
using System.Linq.Expressions;
using System.Reflection;
#endregion

namespace Shin.Framework.Extensions
{
    public static class ExpressionExtensions
    {
        #region Methods
        public static MemberInfo GetMemberInfo(this Expression expression)
        {
            var lambdaExpression = expression as LambdaExpression;
            return
                (lambdaExpression != null && !(lambdaExpression.Body is UnaryExpression)
                     ? (MemberExpression)lambdaExpression.Body
                     : (MemberExpression)((UnaryExpression)lambdaExpression.Body).Operand).Member;
        }

        public static string GetMemberName<T>(this Expression<Func<T>> memberExpression)
        {
            if (memberExpression.Body is MemberExpression expressionBody)
                return expressionBody.Member.Name;

            throw new InvalidOperationException("Expression body must be the MemberExpression type.");
        }
        #endregion
    }
}