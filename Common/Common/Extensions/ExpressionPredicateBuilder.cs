using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Common.Extensions
{
    public static class ExpressionPredicateBuilder
    {
        static Expression<T> Combine<T>(this Expression<T> first, Expression<T> second, Func<Expression, Expression, Expression> combineFunc)
        {
            var secondBody = ParameterRebinderVisitor.ReplaceParameters(first, second);
            return Expression.Lambda<T>(combineFunc(first.Body, secondBody), first.Parameters);
        }

        public static Expression<Func<T, bool>> AndAlso<T>(this Expression<Func<T, bool>> first, Expression<Func<T, bool>> second) =>
            first.Combine(second, Expression.AndAlso);

        public static Expression<Func<T, bool>> OrElse<T>(this Expression<Func<T, bool>> first, Expression<Func<T, bool>> second) =>
            first.Combine(second, Expression.OrElse);

        public static Expression<Func<T, bool>> Not<T>(this Expression<Func<T, bool>> expression) =>
            Expression.Lambda<Func<T, bool>>(Expression.Not(expression.Body), expression.Parameters);

        public static Expression<Func<T, bool>> True<T>() => param => true;
        public static Expression<Func<T, bool>> False<T>() => param => false;

        class ParameterRebinderVisitor : ExpressionVisitor
        {
            readonly Dictionary<ParameterExpression, ParameterExpression> map;

            ParameterRebinderVisitor(Dictionary<ParameterExpression, ParameterExpression> map)
            {
                this.map = map ?? new Dictionary<ParameterExpression, ParameterExpression>();
            }

            public static Expression ReplaceParameters(Dictionary<ParameterExpression, ParameterExpression> map, Expression exp) =>
                new ParameterRebinderVisitor(map).Visit(exp);

            public static Expression ReplaceParameters<T>(Expression<T> exp1, Expression<T> exp2)
            {
                var map = exp1.Parameters
                    .Select((f, i) => new { f, s = exp2.Parameters[i] })
                    .ToDictionary(p => p.s, p => p.f);

                return ReplaceParameters(map, exp2.Body);
            }

            protected override Expression VisitParameter(ParameterExpression p)
            {
                if(map.TryGetValue(p, out ParameterExpression replacement))
                {
                    p = replacement;
                }

                return base.VisitParameter(p);
            }
        }
    }
}
