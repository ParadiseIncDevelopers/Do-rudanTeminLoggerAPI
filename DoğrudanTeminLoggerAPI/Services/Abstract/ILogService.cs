using DoğrudanTeminLoggerAPI.Helpers;
using DoğrudanTeminLoggerAPI.Models;

namespace DoğrudanTeminLoggerAPI.Services.Abstract
{
    public interface ILogService
    {
        Task LogAsync(LogEntryLogRequest request);
        Task<IEnumerable<LogEntry>> QueryAsync(LogQueryParameters parameters);
    }
}
