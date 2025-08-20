using Microsoft.EntityFrameworkCore;
using Portal.Services.Interfaces;
using Portal.Shared.Models.DTOs.Shared;
using Portal.Shared.Models.Entities;
using System.Linq.Expressions;

namespace Portal.Services.Models
{
    public class AuditLogService(PortalDbContext context) : IAuditLogService
    {

        /// <inheritdoc/>
        public async Task<PagedResult<AuditLog>> GetAuditLogsAsync(int pageNumber, int pageSize, string? username, string? tableName, DateTime? startDate, DateTime? endDate)
        {
            var query = context.AuditLogs.AsNoTracking().OrderByDescending(a => a.DateTime);

            var predicates = new List<Expression<Func<AuditLog, bool>>>();

            if (!string.IsNullOrWhiteSpace(username))
            {
                predicates.Add(a => a.Username != null && a.Username.Contains(username));
            }

            if (!string.IsNullOrWhiteSpace(tableName))
            {
                predicates.Add(a => a.TableName == tableName);
            }

            if (startDate.HasValue)
            {
                predicates.Add(a => a.DateTime >= startDate.Value.ToUniversalTime());
            }

            if (endDate.HasValue)
            {
                var inclusiveEndDate = endDate.Value.Date.AddDays(1).AddTicks(-1);
                predicates.Add(a => a.DateTime <= inclusiveEndDate.ToUniversalTime());
            }

            IQueryable<AuditLog> filteredQuery = query;
            if (predicates.Count != 0)
            {
                var finalPredicate = predicates.Aggregate((current, next) => current.And(next));
                filteredQuery = query.Where(finalPredicate);
            }

            var totalRecords = await filteredQuery.CountAsync();

            var logs = await filteredQuery
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PagedResult<AuditLog>
            {
                Items = logs,
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalRecords = totalRecords
            };
        }
    }

    internal static class PredicateBuilder
    {
        public static Expression<Func<T, bool>> And<T>(this Expression<Func<T, bool>> a, Expression<Func<T, bool>> b)
        {
            var parameter = a.Parameters[0];
            var visitor = new SubstExpressionVisitor { Subst = { [b.Parameters[0]] = parameter } };
            var body = Expression.AndAlso(a.Body, visitor.Visit(b.Body));
            return Expression.Lambda<Func<T, bool>>(body, parameter);
        }

        private class SubstExpressionVisitor : ExpressionVisitor
        {
            public readonly Dictionary<Expression, Expression> Subst = [];
            protected override Expression VisitParameter(ParameterExpression node)
            {
                return Subst.TryGetValue(node, out var newValue) ? newValue : node;
            }
        }
    }
}

