using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace FRPAMSystem.BusinessTier.Utils
{
    public static class QueryableExtensions
    {
        public static IQueryable<T> WhereIf<T>(
            this IQueryable<T> query,
            bool condition,
            Expression<Func<T, bool>> predicate)
        {
            return condition ? query.Where(predicate) : query;
        }

        public static IQueryable<T> SearchIf<T>(
            this IQueryable<T> query,
            string? keyword,
            params Expression<Func<T, string?>>[] stringProperties)
        {
            if (string.IsNullOrWhiteSpace(keyword))
            {
                return query;
            }

            keyword = keyword.Trim();

            var parameter = Expression.Parameter(typeof(T), "x");

            Expression? combinedExpression = null;

            foreach (var propertyExpression in stringProperties)
            {
                var propertyBody = ReplaceParameter(
                    propertyExpression.Body,
                    propertyExpression.Parameters[0],
                    parameter
                );

                var notNullExpression = Expression.NotEqual(
                    propertyBody,
                    Expression.Constant(null, typeof(string))
                );

                var containsMethod = typeof(string).GetMethod(
                    nameof(string.Contains),
                    new[] { typeof(string) }
                )!;

                var containsExpression = Expression.Call(
                    propertyBody,
                    containsMethod,
                    Expression.Constant(keyword)
                );

                var safeContainsExpression = Expression.AndAlso(
                    notNullExpression,
                    containsExpression
                );

                combinedExpression = combinedExpression == null
                    ? safeContainsExpression
                    : Expression.OrElse(combinedExpression, safeContainsExpression);
            }

            if (combinedExpression == null)
            {
                return query;
            }

            var lambda = Expression.Lambda<Func<T, bool>>(
                combinedExpression,
                parameter
            );

            return query.Where(lambda);
        }

        public static IQueryable<T> WhereEqualsIf<T, TValue>(
            this IQueryable<T> query,
            TValue? value,
            Expression<Func<T, TValue>> propertySelector)
            where TValue : struct
        {
            if (!value.HasValue)
            {
                return query;
            }

            var parameter = propertySelector.Parameters[0];

            var body = Expression.Equal(
                propertySelector.Body,
                Expression.Constant(value.Value)
            );

            var lambda = Expression.Lambda<Func<T, bool>>(body, parameter);

            return query.Where(lambda);
        }

        public static IQueryable<T> WhereStringEqualsIf<T>(
            this IQueryable<T> query,
            string? value,
            Expression<Func<T, string>> propertySelector)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return query;
            }

            value = value.Trim();

            var parameter = propertySelector.Parameters[0];

            var body = Expression.Equal(
                propertySelector.Body,
                Expression.Constant(value)
            );

            var lambda = Expression.Lambda<Func<T, bool>>(body, parameter);

            return query.Where(lambda);
        }

        public static IQueryable<T> WhereDateFromIf<T>(
            this IQueryable<T> query,
            DateTime? from,
            Expression<Func<T, DateTime>> propertySelector)
        {
            if (!from.HasValue)
            {
                return query;
            }

            var parameter = propertySelector.Parameters[0];

            var body = Expression.GreaterThanOrEqual(
                propertySelector.Body,
                Expression.Constant(from.Value)
            );

            var lambda = Expression.Lambda<Func<T, bool>>(body, parameter);

            return query.Where(lambda);
        }

        public static IQueryable<T> WhereDateToIf<T>(
            this IQueryable<T> query,
            DateTime? to,
            Expression<Func<T, DateTime>> propertySelector)
        {
            if (!to.HasValue)
            {
                return query;
            }

            var parameter = propertySelector.Parameters[0];

            var body = Expression.LessThanOrEqual(
                propertySelector.Body,
                Expression.Constant(to.Value)
            );

            var lambda = Expression.Lambda<Func<T, bool>>(body, parameter);

            return query.Where(lambda);
        }

        public static IQueryable<T> WhereNullableDateFromIf<T>(
            this IQueryable<T> query,
            DateTime? from,
            Expression<Func<T, DateTime?>> propertySelector)
        {
            if (!from.HasValue)
            {
                return query;
            }

            var parameter = propertySelector.Parameters[0];

            var hasValueExpression = Expression.Property(
                propertySelector.Body,
                nameof(Nullable<DateTime>.HasValue)
            );

            var valueExpression = Expression.Property(
                propertySelector.Body,
                nameof(Nullable<DateTime>.Value)
            );

            var compareExpression = Expression.GreaterThanOrEqual(
                valueExpression,
                Expression.Constant(from.Value)
            );

            var body = Expression.AndAlso(
                hasValueExpression,
                compareExpression
            );

            var lambda = Expression.Lambda<Func<T, bool>>(body, parameter);

            return query.Where(lambda);
        }

        public static IQueryable<T> WhereNullableDateToIf<T>(
            this IQueryable<T> query,
            DateTime? to,
            Expression<Func<T, DateTime?>> propertySelector)
        {
            if (!to.HasValue)
            {
                return query;
            }

            var parameter = propertySelector.Parameters[0];

            var hasValueExpression = Expression.Property(
                propertySelector.Body,
                nameof(Nullable<DateTime>.HasValue)
            );

            var valueExpression = Expression.Property(
                propertySelector.Body,
                nameof(Nullable<DateTime>.Value)
            );

            var compareExpression = Expression.LessThanOrEqual(
                valueExpression,
                Expression.Constant(to.Value)
            );

            var body = Expression.AndAlso(
                hasValueExpression,
                compareExpression
            );

            var lambda = Expression.Lambda<Func<T, bool>>(body, parameter);

            return query.Where(lambda);
        }

        private static Expression ReplaceParameter(
            Expression expression,
            ParameterExpression source,
            ParameterExpression target)
        {
            return new ParameterReplaceVisitor(source, target)
                .Visit(expression)!;
        }

        private class ParameterReplaceVisitor : ExpressionVisitor
        {
            private readonly ParameterExpression _source;
            private readonly ParameterExpression _target;

            public ParameterReplaceVisitor(
                ParameterExpression source,
                ParameterExpression target)
            {
                _source = source;
                _target = target;
            }

            protected override Expression VisitParameter(ParameterExpression node)
            {
                return node == _source ? _target : base.VisitParameter(node);
            }
        }
        public static IQueryable<T> WhereNullableEqualsIf<T, TValue>(
    this IQueryable<T> query,
    TValue? value,
    Expression<Func<T, TValue?>> propertySelector)
    where TValue : struct
        {
            if (!value.HasValue)
            {
                return query;
            }

            var parameter = propertySelector.Parameters[0];

            var hasValueExpression = Expression.Property(
                propertySelector.Body,
                nameof(Nullable<TValue>.HasValue)
            );

            var valueExpression = Expression.Property(
                propertySelector.Body,
                nameof(Nullable<TValue>.Value)
            );

            var equalExpression = Expression.Equal(
                valueExpression,
                Expression.Constant(value.Value)
            );

            var body = Expression.AndAlso(
                hasValueExpression,
                equalExpression
            );

            var lambda = Expression.Lambda<Func<T, bool>>(body, parameter);

            return query.Where(lambda);
        }
    }
}
