using Microsoft.EntityFrameworkCore;
using Portal.Services.Interfaces;
using Portal.Shared.Models.DTOs.Shared;
using Portal.Shared.Models.Entities;
using System.Linq.Expressions;

namespace Portal.Services.Models
{
    /// <summary>
    /// Service for retrieving audit log information from the database.
    /// The creation of logs is handled automatically by PortalDbContext.
    /// </summary>
    public class AuditLogService(PortalDbContext context) : IAuditLogService
    {

        /// <inheritdoc/>
        public async Task<PagedResult<AuditLog>> GetAuditLogsAsync(int pageNumber, int pageSize, string? username, string? tableName, DateTime? startDate, DateTime? endDate)
        {
            // Start with the base query, ordering by the most recent logs first.
            var query = context.AuditLogs.AsNoTracking().OrderByDescending(a => a.DateTime);

            // Build a list of filters (predicates)
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
                // Add 1 day and subtract a tick to include the entire end day
                var inclusiveEndDate = endDate.Value.Date.AddDays(1).AddTicks(-1);
                predicates.Add(a => a.DateTime <= inclusiveEndDate.ToUniversalTime());
            }

            // Apply all filters to the query
            IQueryable<AuditLog> filteredQuery = query;
            if (predicates.Count != 0)
            {
                // Combine all predicates with 'AND'
                var finalPredicate = predicates.Aggregate((current, next) => current.And(next));
                filteredQuery = query.Where(finalPredicate);
            }

            // Get total count for pagination
            var totalRecords = await filteredQuery.CountAsync();

            // Get the paginated data
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

    // Helper class to combine Expression trees
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

