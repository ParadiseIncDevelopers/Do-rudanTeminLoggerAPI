using DoğrudanTeminLoggerAPI.Helpers;
using DoğrudanTeminLoggerAPI.Models;
using System.Collections.Generic;

namespace DoğrudanTeminLoggerAPI.Services.Abstract
{
    public interface IPageService
    {
        Task PageAsync(PageEntryLogRequest request);
        Task<IEnumerable<PageEntry>> QueryAsync(PageQueryParameters parameters);
    }
}
