using AutoMapper;
using DoğrudanTeminLoggerAPI.Dto;
using DoğrudanTeminLoggerAPI.Helpers;
using DoğrudanTeminLoggerAPI.Services.Abstract;
using Microsoft.AspNetCore.Mvc;

namespace DoğrudanTeminLoggerAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class LogsController : ControllerBase
    {
        private readonly ILogService _logService;
        private readonly IMapper _mapper;

        public LogsController(ILogService logService, IMapper mapper)
        {
            _logService = logService;
            _mapper = mapper;
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] LogEntryDto dto)
        {
            var request = new LogEntryLogRequest
            {
                LogDateTime = dto.LogDateTime,
                LogText = dto.LogText,
                LogDescription = dto.LogDescription,
                LogObject = dto.LogObject,
                LogIP = dto.LogIP,
                UserId = dto.UserId,
                Token = dto.Token
            };
            await _logService.LogAsync(request);
            return Ok();
        }

        [HttpGet]
        public async Task<IActionResult> Get([FromQuery] LogQueryParameters q)
        {
            var entries = await _logService.QueryAsync(q);
            var dtos = _mapper.Map<IEnumerable<LogEntryDto>>(entries);
            return Ok(dtos);
        }
    }
}
