using System.Linq.Expressions;

namespace Atlas.Common.Validation.Core;

internal static class PropertyExpressionHelper
{
    internal static string GetPropertyName<T, TProperty>(Expression<Func<T, TProperty>> expression)
    {
        if (expression.Body is MemberExpression member)
            return member.Member.Name;

        if (expression.Body is UnaryExpression { Operand: MemberExpression unaryMember })
            return unaryMember.Member.Name;

        return expression.Body.ToString();
    }
}
