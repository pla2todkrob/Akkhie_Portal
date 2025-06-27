using Portal.Shared.Models.DTOs.Shared;
using Portal.Shared.Models.Entities;

namespace Portal.Services.Interfaces
{
    /// <summary>
    /// Interface for querying audit log data.
    /// </summary>
    public interface IAuditLogService
    {
        /// <summary>
        /// Gets a paginated list of audit logs based on the specified criteria.
        /// </summary>
        /// <param name="pageNumber">The page number to retrieve.</param>
        /// <param name="pageSize">The number of records per page.</param>
        /// <param name="username">Optional filter by username.</param>
        /// <param name="tableName">Optional filter by table name.</param>
        /// <param name="startDate">Optional start date for the filter range.</param>
        /// <param name="endDate">Optional end date for the filter range.</param>
        /// <returns>A paged result of audit logs.</returns>
        Task<PagedResult<AuditLog>> GetAuditLogsAsync(int pageNumber, int pageSize, string? username, string? tableName, DateTime? startDate, DateTime? endDate);
    }
}
